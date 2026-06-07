using ECommerceAuction.UserService.Api.GrpcServices;
using ECommerceAuction.UserService.Api.Middlewares;
using ECommerceAuction.UserService.Application;
using ECommerceAuction.UserService.Infrastructure;
using ECommerceAuction.UserService.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddGrpc();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("ReactVite");

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<InternalHealthGrpcService>();
app.MapControllers();

app.Run();

