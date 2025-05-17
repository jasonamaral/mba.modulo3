using FluencyHub.API;
using FluencyHub.API.Middleware;
using FluencyHub.Application;
using FluencyHub.ContentManagement.Application;
using FluencyHub.StudentManagement.Application;
using FluencyHub.PaymentProcessing.Application;
using FluencyHub.Infrastructure;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplicationServices();
builder.Services.AddContentManagementApplication();
builder.Services.AddStudentManagementApplication();
builder.Services.AddPaymentProcessingApplication();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
{
    app.UseSwaggerConfiguration();
}

try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<FluencyHubDbContext>();
    var identityDbContext = services.GetRequiredService<ApplicationDbContext>();

    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
    {

        // Development or Testing Environment - Using SQLite
        var mainDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
        var identityDbConnectionString = builder.Configuration.GetConnectionString("IdentityConnection") ?? "";

        bool isMemoryDb = mainDbConnectionString.Contains(":memory:") || identityDbConnectionString.Contains(":memory:");

        if (!isMemoryDb)
        {
            string mainDbPath = mainDbConnectionString.Replace("Data Source=", "").Trim();
            string identityDbPath = identityDbConnectionString.Replace("Data Source=", "").Trim();

            // Ensure the Data directory exists
            var dataDir = Path.GetDirectoryName(mainDbPath);
            if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            // Check if database files exist
            bool mainDbExists = !string.IsNullOrEmpty(mainDbPath) && File.Exists(mainDbPath);
            bool identityDbExists = !string.IsNullOrEmpty(identityDbPath) && File.Exists(identityDbPath);

        }

        dbContext.Database.Migrate();
        identityDbContext.Database.Migrate();
        await DatabaseSeeder.SeedData(services);
    }
    else
    {

        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }

        if (identityDbContext.Database.GetPendingMigrations().Any())
        {
            identityDbContext.Database.Migrate();
        }

        if (!dbContext.Set<FluencyHub.Domain.ContentManagement.Course>().Any())
        {
            await DatabaseSeeder.SeedData(services);
        }

    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error initializing database");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Making the Program class public and partial for testing
public partial class Program
{ }