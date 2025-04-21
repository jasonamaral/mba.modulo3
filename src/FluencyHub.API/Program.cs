using FluencyHub.API;
using FluencyHub.API.Middleware;
using FluencyHub.Application;
using FluencyHub.Infrastructure;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwaggerConfiguration();

// Initialize database based on environment
bool isSQLiteProfile = Environment.GetEnvironmentVariable("DatabaseProvider") == "SQLite";

try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<FluencyHubDbContext>();
        var identityDbContext = services.GetRequiredService<ApplicationDbContext>();

        // Check if SQLite profile is active
        if (isSQLiteProfile)
        {
            logger.LogInformation("SQLite profile detected. Checking database...");
            
            // Get the database paths from connection strings
            var mainDbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
            var identityDbConnectionString = builder.Configuration.GetConnectionString("IdentityConnection") ?? "";
            
            string mainDbPath = mainDbConnectionString.Replace("Data Source=", "").Trim();
            string identityDbPath = identityDbConnectionString.Replace("Data Source=", "").Trim();
            
            // Ensure the Data directory exists
            var dataDir = Path.GetDirectoryName(mainDbPath);
            if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            
            // Check if the database files exist
            bool mainDbExists = !string.IsNullOrEmpty(mainDbPath) && File.Exists(mainDbPath);
            bool identityDbExists = !string.IsNullOrEmpty(identityDbPath) && File.Exists(identityDbPath);
            
            if (!mainDbExists || !identityDbExists)
            {
                logger.LogInformation("Database files not found. Applying migrations and seeding data...");
                
                // Apply migrations
                dbContext.Database.Migrate();
                identityDbContext.Database.Migrate();
                
                // Seed data
                await DatabaseSeeder.SeedData(services);
                
                logger.LogInformation("Database initialization completed successfully!");
            }
            else
            {
                logger.LogInformation("SQLite database files already exist. Skipping initialization.");
            }
        }
        else if (app.Environment.IsDevelopment())
        {
            // For non-SQLite development environment
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            
            // Apply migrations in development environment
            dbContext.Database.Migrate();
            identityDbContext.Database.Migrate();
            
            // Seed data
            await DatabaseSeeder.SeedData(services);
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error initializing the database");
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class public and partial for testing
public partial class Program
{ }