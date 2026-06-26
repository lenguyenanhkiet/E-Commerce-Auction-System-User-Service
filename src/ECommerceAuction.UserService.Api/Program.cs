using ECommerceAuction.UserService.Api.GrpcServices;
using ECommerceAuction.UserService.Api.Middlewares;
using ECommerceAuction.UserService.Api.Authorization;
using ECommerceAuction.UserService.Application;
using ECommerceAuction.UserService.Infrastructure;
using ECommerceAuction.UserService.Persistence;
using MassTransit;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMqConnection");

        cfg.Host(rabbitMqConnectionString);

        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddControllers();

builder.Services.AddGrpc();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PrivilegePolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PrivilegeAuthorizationHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT access token. Example: Bearer eyJhbGciOi..."
    });

    // Swagger sends the authorized JWT to protected RBAC APIs.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactVite", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseMiddleware<ExceptionHandlingMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("ReactVite");

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<InternalHealthGrpcService>();
app.MapControllers();

//app.Run();
try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("\n========================================================");
    Console.WriteLine($"[FATAL CRASH] Thủ phạm làm sập App: {ex.Message}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"[INNER EXCEPTION] Chi tiết sâu hơn: {ex.InnerException.Message}");
    }

    Console.WriteLine("========================================================\n");
    throw;
}

