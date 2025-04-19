using FluencyHub.API;
using FluencyHub.API.Middleware;
using FluencyHub.Application;
using FluencyHub.Infrastructure;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpsRedirection();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FluencyHubDbContext>();
    dbContext.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed database
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting database seed...");
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<FluencyHubDbContext>();

        await DatabaseSeeder.SeedData(services);
    }
    logger.LogInformation("Database seed completed successfully!");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error executing database seed");
}

app.Run();

// Make Program class public and partial for testing
public partial class Program
{ }