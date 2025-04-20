using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluencyHub.Infrastructure.Persistence;
using FluencyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using FluencyHub.API;
using Microsoft.Data.Sqlite;

namespace FluencyHub.Tests.Integration.Config;

public class FluencyHubAppFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private SqliteConnection _mainConnection;
    private SqliteConnection _identityConnection;

    public FluencyHubAppFactory()
    {
        _mainConnection = new SqliteConnection("DataSource=:memory:");
        _mainConnection.Open();

        _identityConnection = new SqliteConnection("DataSource=:memory:");
        _identityConnection.Open();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Replace the main database with in-memory SQLite
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FluencyHubDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<FluencyHubDbContext>(options =>
            {
                options.UseSqlite(_mainConnection);
            });

            // Replace the identity database with in-memory SQLite
            var identityDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (identityDescriptor != null)
            {
                services.Remove(identityDescriptor);
            }
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_identityConnection);
            });

            // Create and seed the databases
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var mainDb = scopedServices.GetRequiredService<FluencyHubDbContext>();
                var identityDb = scopedServices.GetRequiredService<ApplicationDbContext>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();

                mainDb.Database.EnsureCreated();
                identityDb.Database.EnsureCreated();

                // Seed roles
                if (!roleManager.RoleExistsAsync("Administrator").Result)
                {
                    roleManager.CreateAsync(new IdentityRole("Administrator")).Wait();
                }
                if (!roleManager.RoleExistsAsync("Student").Result)
                {
                    roleManager.CreateAsync(new IdentityRole("Student")).Wait();
                }

                // Seed admin user
                var adminEmail = "admin@fluencyhub.com";
                var adminUser = userManager.FindByEmailAsync(adminEmail).Result;
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        EmailConfirmed = true
                    };
                    var result = userManager.CreateAsync(adminUser, "Test@123").Result;
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(adminUser, "Administrator").Wait();
                    }
                }
            }
        });

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mainConnection?.Dispose();
            _identityConnection?.Dispose();
        }

        base.Dispose(disposing);
    }
}