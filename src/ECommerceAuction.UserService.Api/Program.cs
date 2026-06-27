//Enter the necessary namespaces for the application
using ECommerceAuction.UserService.Api.GrpcServices;
using ECommerceAuction.UserService.Api.Middlewares;
using ECommerceAuction.UserService.Application;
using ECommerceAuction.UserService.Infrastructure;
using ECommerceAuction.UserService.Persistence;
using MassTransit;
using Microsoft.OpenApi;

//Create a builder to configure the web application's services
var builder = WebApplication.CreateBuilder(args);

//Configure MassTransit to use RabbitMq as the message broker
//MassTransit is a library that helps handle sending/receiving messages from queues
builder.Services.AddMassTransit(x =>
{
    //Configure to use RabbitMq as service transport
    x.UsingRabbitMq((context, cfg) =>
    {
        //Get the RabbitMq connection string from the appsettings.json file
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMqConnection");

        //Set up the RabbitMq host to connect to
        cfg.Host(rabbitMqConnectionString);

        //Configure endpoints to automatically map messages
        cfg.ConfigureEndpoints(context);
    });
});


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
        //The security scheme uses Bearer tokens
        Scheme = "bearer",
        //The format of the token is JWT
        BearerFormat = "JWT",
        //Location of the security header
        In = ParameterLocation.Header,
        //Description of usage
        Description = "Enter JWT access token. Example: Bearer eyJhbGciOi..."
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

//Check if the application runs in a Development environment
//If Development, use Swagger UI to view API documentation
//if (app.Environment.IsDevelopment())
//{
//Enable Swagger middleware - provides JSON schema of the API
app.UseSwagger();
//Enable Swagger UI - a web interface to view and test APIs
app.UseSwaggerUI();
//}

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


//Start running the web application
//Use try-catch to catch unhandled exceptions and log them to the console
try
{
    //Start the web application, listen for HTTP requests
    app.Run();
}
catch (Exception ex)
{
    //Print to the console to notify that the application has crashed
    Console.WriteLine("\n========================================================");
    Console.WriteLine($"[FATAL CRASH] The culprit that crashed the app: {ex.Message}");

    //If there is an InnerException (original error), print more details
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[INNER EXCEPTION] More detailed information: {ex.InnerException.Message}");
    }

    Console.WriteLine("========================================================\n");
    //Throws an error to stop the application
    throw;
}

