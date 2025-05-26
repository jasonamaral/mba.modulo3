using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Identity;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Domain;
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
            // Os bancos de dados já foram criados pelas migrações no Program.cs
            logger.LogInformation("Iniciando o preenchimento dos dados iniciais");

            await SeedRoles(services);
            await SeedUsers(services);
            await SeedStudents(services);
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

        // Usuários estudantes
        var studentEmails = new[]
        {
            "maria.silva@fluencyhub.com",
            "joao.santos@fluencyhub.com",
            "ana.oliveira@fluencyhub.com"
        };

        foreach (var email in studentEmails)
        {
            var studentUser = await userManager.FindByEmailAsync(email);
            if (studentUser == null)
            {
                var nameParts = email.Split('@')[0].Split('.');
                studentUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = char.ToUpper(nameParts[0][0]) + nameParts[0].Substring(1),
                    LastName = char.ToUpper(nameParts[1][0]) + nameParts[1].Substring(1),
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studentUser, "Test@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }
        }
    }

    private static async Task SeedStudents(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var studentDbContext = services.GetRequiredService<StudentDbContext>();

        // Verificar se já existem estudantes
        if (studentDbContext.Students.Any())
        {
            logger.LogInformation("Estudantes já existem no banco de dados");
            return;
        }

        var students = new[]
        {
            new Student("Maria", "Silva", "maria.silva@fluencyhub.com", new DateTime(1995, 5, 15)),
            new Student("João", "Santos", "joao.santos@fluencyhub.com", new DateTime(1992, 8, 22)),
            new Student("Ana", "Oliveira", "ana.oliveira@fluencyhub.com", new DateTime(1998, 3, 10))
        };

        await studentDbContext.Students.AddRangeAsync(students);
        await studentDbContext.SaveChangesAsync();

        logger.LogInformation($"Criados {students.Length} estudantes no banco de dados");
    }
}