//Enter the necessary namespaces for the application
using ECommerceAuction.UserService.Api.GrpcServices;
using ECommerceAuction.UserService.Api.Middlewares;
using ECommerceAuction.UserService.Api.Authorization;
using ECommerceAuction.UserService.Application;
using ECommerceAuction.UserService.Infrastructure;
using ECommerceAuction.UserService.Persistence;
using ECommerceAuction.UserService.Persistence.Context;
using ECommerceAuction.UserService.Persistence.Seeders;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Nexus.Upload.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;

//Create a builder to configure the web application's services
var builder = WebApplication.CreateBuilder(args);
//Register the Controllers service - allows the application to handle HTTP requests
builder.Services.AddControllers();
//Register for the gRPC service - allows the application to support gRPC communication
builder.Services.AddGrpc();
//Register for services from the Application layer
builder.Services.AddApplication();
//Register services from the Persistence layer (database)
builder.Services.AddPersistence(builder.Configuration);
//Register services from the Infrastructure layer (authentication, cache, etc.)
builder.Services.AddInfrastructure(builder.Configuration);
//Register the shared image-upload library (Nexus.Upload). Storage provider is chosen from the
//"UploadService" config section: Local for dev/local-docker, DigitalOcean Spaces in Production.
builder.Services.AddNexusUpload(builder.Configuration);

//Bind account security policy (lockout + password expiry) and start its background sweepers
builder.Services.Configure<ECommerceAuction.UserService.Application.Common.Options.AccountPolicyOptions>(
    builder.Configuration.GetSection(
        ECommerceAuction.UserService.Application.Common.Options.AccountPolicyOptions.SectionName));
builder.Services.AddHostedService<ECommerceAuction.UserService.Api.BackgroundJobs.AccountUnlockSweeperService>();
builder.Services.AddHostedService<ECommerceAuction.UserService.Api.BackgroundJobs.PasswordExpirySweeperService>();
// Dynamic policy provider: any [Authorize(Policy = "...")] name is treated as a permission code
// and checked against the "privilege" claims on the JWT (see PermissionPolicyProvider).
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddEndpointsApiExplorer();

//Configure Swagger/OpenAPI for API documentation
builder.Services.AddSwaggerGen(options =>
{
    //Add security definition for Bearer token (JWT)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        //Name of the security header
        Name = "Authorization",
        //The security type is HTTP
        Type = SecuritySchemeType.Http,

        Scheme = "Bearer",

        BearerFormat = "JWT",
        //Location of the security header
        In = ParameterLocation.Header,
        Description = "Enter JWT access token. Example: Bearer eyJhbGciOi..."
    });

    // Swagger sends the authorized JWT to protected RBAC APIs.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
});

//Configure CORS (Cross-Origin Resource Sharing) - allow frontend access from different domains
builder.Services.AddCors(options =>
{
    //Create a CORS policy named "ReactVite"
    options.AddPolicy("ReactVite", policy =>
    {
        //Allow requests from the React Vite dev server
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            //Allow any header in the request
            .AllowAnyHeader()
            //Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
            .AllowAnyMethod()
            //Allows sending cookies/credentials in requests from clients
            .AllowCredentials();
    });
});

// Kestrel only ever listens on plain HTTP (see Kestrel:Endpoints); TLS terminates upstream at the
// gateway or the load balancer, which forwards the original scheme in X-Forwarded-Proto. Without
// this, every request looks like http to the app: absolute URLs it generates come out http, and
// RemoteIpAddress is the proxy rather than the caller.
//
// It also has to run before UseHttpsRedirection. That middleware needs a target port and quietly
// does nothing without one, which is why plain Production currently neither loops nor enforces
// HTTPS — but the moment an https port is configured (ASPNETCORE_HTTPS_PORT, or an https endpoint),
// it starts 307-ing http requests back at the proxy, which forwards them as http again. Verified:
// with ASPNETCORE_HTTPS_PORT=443 and no forwarded header a request 307s to https; with the header
// it is served.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // By default only loopback proxies are trusted, and the proxy here is another container or the
    // load balancer — never loopback — so the headers would be dropped and the redirect loop would
    // survive the fix. Clearing the lists trusts whatever forwards to us, which is only safe while
    // the container is not reachable directly from the internet: publish it behind the gateway only.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

//Build web applications from the builder
var app = builder.Build();

app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

//Use custom error handling middleware to catch unexpected exceptions
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Serve files saved by the Local storage provider at /uploads (no effect for the Spaces provider,
// which returns absolute DigitalOcean URLs). Physical folder matches UploadService:Local:BaseDirectory.
var uploadsPath = Path.Combine(
    app.Environment.ContentRootPath,
    builder.Configuration["UploadService:Local:BaseDirectory"] ?? "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

//If it is not a Development environment, force the use of HTTPS

if (!app.Environment.IsDevelopment())
{
    //Redirect all HTTP requests to HTTPS
    app.UseHttpsRedirection();
}

//Use the "ReactVite" CORS policy configured above
app.UseCors("ReactVite");

//Use authentication middleware - verify who the user is
app.UseAuthentication();
//Use authorization middleware - verify what the user has the right to do
app.UseAuthorization();

//Register gRPC service for health check
app.MapGrpcService<InternalHealthGrpcService>();
app.MapGrpcService<UserSellerEligibilityGrpcService>();
app.MapGrpcService<UserProfileGrpcService>();
app.MapGrpcService<UserCommerceGrpcService>();
app.MapGrpcService<ReputationEligibilityGrpcService>();
//Register all controller endpoints
app.MapControllers();
// Database migration AND permission-catalog seeding both run on startup in the
// DatabaseMigrationService hosted service (Infrastructure), in that order — migrate first, then
// seed — so the schema exists before seeding. Nothing schema-related runs here in Program.cs.
app.Run();
