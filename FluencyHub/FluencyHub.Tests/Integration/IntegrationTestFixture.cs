using System.Data.Common;
using FluencyHub.API;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluencyHub.Tests.Integration;

public class IntegrationTestFixture : WebApplicationFactory<Program>, IDisposable
{
    private readonly DbConnection _domainConnection;
    private readonly DbConnection _identityConnection;
    
    public IntegrationTestFixture()
    {
        _domainConnection = new SqliteConnection("DataSource=:memory:");
        _domainConnection.Open();
        
        _identityConnection = new SqliteConnection("DataSource=:memory:");
        _identityConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the app's ApplicationDbContext registration
            var domainDbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FluencyHubDbContext>));
            
            if (domainDbContextDescriptor != null)
            {
                services.Remove(domainDbContextDescriptor);
            }
            
            var identityDbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            
            if (identityDbContextDescriptor != null)
            {
                services.Remove(identityDbContextDescriptor);
            }
            
            // Add ApplicationDbContext using an in-memory database for testing
            services.AddDbContext<FluencyHubDbContext>(options =>
            {
                options.UseSqlite(_domainConnection);
            });
            
            // Add IdentityDbContext using an in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_identityConnection);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var domainDb = scopedServices.GetRequiredService<FluencyHubDbContext>();
            var identityDb = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<IntegrationTestFixture>>();

            // Ensure the database is created
            domainDb.Database.EnsureCreated();
            identityDb.Database.EnsureCreated();

            try
            {
                // Seed the database with test data if needed
                SeedIdentityData(scopedServices).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database. Error: {Message}", ex.Message);
            }
        });
    }

    private async Task SeedIdentityData(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var studentRepository = serviceProvider.GetRequiredService<FluencyHub.Application.Common.Interfaces.IStudentRepository>();

        // Seed roles
        string[] roles = { "Administrator", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin@fluencyhub.com",
            Email = "admin@fluencyhub.com",
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        if (await userManager.FindByEmailAsync(adminUser.Email) == null)
        {
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
        
        // Seed student user
        var studentUser = new ApplicationUser
        {
            UserName = "student@example.com",
            Email = "student@example.com",
            FirstName = "Test",
            LastName = "Student",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        if (await userManager.FindByEmailAsync(studentUser.Email) == null)
        {
            await userManager.CreateAsync(studentUser, "Student123!");
            await userManager.AddToRoleAsync(studentUser, "Student");
            
            // Create student entity in domain database
            var student = new FluencyHub.Domain.StudentManagement.Student(
                studentUser.FirstName,
                studentUser.LastName,
                studentUser.Email,
                DateTime.Now.AddYears(-20),
                "+1234567890"
            );
            
            await studentRepository.AddAsync(student);
            await studentRepository.SaveChangesAsync();
        }
    }

    public new void Dispose()
    {
        _domainConnection.Dispose();
        _identityConnection.Dispose();
        base.Dispose();
    }
} 