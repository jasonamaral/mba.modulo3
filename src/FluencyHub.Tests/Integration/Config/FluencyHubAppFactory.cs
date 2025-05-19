using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluencyHub.Infrastructure.Persistence;
using FluencyHub.Infrastructure.Persistence.Extensions;
using FluencyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using FluencyHub.API;
using Microsoft.Data.Sqlite;

namespace FluencyHub.Tests.Integration.Config;

public class FluencyHubAppFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly SqliteConnection _mainConnection;
    private readonly SqliteConnection _identityConnection;

    public FluencyHubAppFactory()
    {
        _mainConnection = new SqliteConnection("DataSource=:memory:");
        _mainConnection.Open();

        _identityConnection = new SqliteConnection("DataSource=:memory:");
        _identityConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remover todos os registros de serviços de DbContext
            RemoveDbContextRegistrations(services);

            // Adicionar SQLite em memória para testes
            services.AddDbContext<FluencyHubDbContext>(options =>
            {
                options.UseSqlite(_mainConnection);
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_identityConnection);
            });

            // Criar esquemas de banco de dados
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var mainDb = scopedServices.GetRequiredService<FluencyHubDbContext>();
                var identityDb = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Garantir que os esquemas de banco de dados foram criados
                mainDb.Database.EnsureCreated();
                identityDb.Database.EnsureCreated();

                // Seed dados de identidade
                SeedIdentityData(scopedServices).Wait();
            }
        });

        base.ConfigureWebHost(builder);
    }

    private static async Task SeedIdentityData(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Criar roles
        if (!await roleManager.RoleExistsAsync("Administrator"))
        {
            await roleManager.CreateAsync(new IdentityRole("Administrator"));
        }
        
        if (!await roleManager.RoleExistsAsync("Student"))
        {
            await roleManager.CreateAsync(new IdentityRole("Student"));
        }

        if (!await roleManager.RoleExistsAsync("Teacher"))
        {
            await roleManager.CreateAsync(new IdentityRole("Teacher"));
        }

        // Criar usuário administrador
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

    private static void RemoveDbContextRegistrations(IServiceCollection services)
    {
        // Remover serviços de DbContext registrados anteriormente
        var descriptorsToRemove = services
            .Where(d => (d.ServiceType == typeof(DbContextOptions) ||
                         d.ServiceType.IsGenericType && 
                         d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }
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