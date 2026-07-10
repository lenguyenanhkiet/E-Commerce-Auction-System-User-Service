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

//Build web applications from the builder
var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

//Use custom error handling middleware to catch unexpected exceptions
app.UseMiddleware<ExceptionHandlingMiddleware>();

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
//Register all controller endpoints
app.MapControllers();
// Auto Migration when app run
if (app.Environment.IsProduction() || app.Environment.EnvironmentName == "Staging")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Seed the permission catalog (Privileges table) from the compile-time Permissions class, every
// startup, in every environment — authorization depends on these rows existing. A failure here
// (e.g. schema not migrated yet in Development) must not take the whole service down.
try
{
    using var permissionScope = app.Services.CreateScope();
    var dbContext = permissionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await PermissionSeeder.SeedAsync(dbContext);
}
catch (Exception exception)
{
    app.Logger.LogWarning(
        exception,
        "Failed to seed the permission catalog. Permission-protected endpoints may reject valid requests until this is resolved.");
}

app.Run();


