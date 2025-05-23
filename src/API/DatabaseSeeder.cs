using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Identity;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
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
            // Inicializar cada banco de dados específico por contexto
            var contentContext = services.GetRequiredService<ContentDbContext>();
            var studentContext = services.GetRequiredService<StudentDbContext>();
            var paymentContext = services.GetRequiredService<PaymentDbContext>();
            var identityContext = services.GetRequiredService<ApplicationDbContext>();

            if (contentContext.Database.EnsureCreated())
            {
                logger.LogInformation("Banco de dados de conteúdo criado com sucesso");
            }
            else
            {
                logger.LogInformation("Banco de dados de conteúdo já existe");
            }

            if (studentContext.Database.EnsureCreated())
            {
                logger.LogInformation("Banco de dados de estudantes criado com sucesso");
            }
            else
            {
                logger.LogInformation("Banco de dados de estudantes já existe");
            }

            if (paymentContext.Database.EnsureCreated())
            {
                logger.LogInformation("Banco de dados de pagamentos criado com sucesso");
            }
            else
            {
                logger.LogInformation("Banco de dados de pagamentos já existe");
            }

            if (identityContext.Database.EnsureCreated())
            {
                logger.LogInformation("Banco de dados de identidade criado com sucesso");
            }
            else
            {
                logger.LogInformation("Banco de dados de identidade já existe");
            }

            await SeedRoles(services);
            await SeedUsers(services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro durante o preenchimento do banco de dados");
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

        // Usuário administrador
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