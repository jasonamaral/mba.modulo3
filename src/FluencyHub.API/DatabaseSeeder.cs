using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace FluencyHub.API;

public static class DatabaseSeeder
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<FluencyHubDbContext>();

            if (context.Database.EnsureCreated())
            {
                logger.LogInformation("Main database created successfully");
            }
            else
            {
                logger.LogInformation("Main database already exists");
            }

            var identityContext = services.GetRequiredService<ApplicationDbContext>();

            if (identityContext.Database.EnsureCreated())
            {
                logger.LogInformation("Identity database created successfully");
            }
            else
            {
                logger.LogInformation("Identity database already exists");
            }

            await SeedRoles(services);
            await SeedUsers(services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding");
            throw;
        }
    }

    private static async Task SeedRoles(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Administrator", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsers(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Admin user
        var adminEmail = "admin@fluencyhub.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
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

            var result = await userManager.CreateAsync(adminUser, "Test@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}