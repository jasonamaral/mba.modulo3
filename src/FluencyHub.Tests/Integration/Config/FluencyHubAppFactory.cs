using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluencyHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using FluencyHub.API;
using Microsoft.Data.Sqlite;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence;

namespace FluencyHub.Tests.Integration.Config;

public class FluencyHubAppFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly SqliteConnection _contentConnection;
    private readonly SqliteConnection _studentConnection;
    private readonly SqliteConnection _paymentConnection;
    private readonly SqliteConnection _identityConnection;

    public FluencyHubAppFactory()
    {
        _contentConnection = new SqliteConnection("DataSource=:memory:");
        _contentConnection.Open();
        
        _studentConnection = new SqliteConnection("DataSource=:memory:");
        _studentConnection.Open();
        
        _paymentConnection = new SqliteConnection("DataSource=:memory:");
        _paymentConnection.Open();

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
            services.AddDbContext<ContentDbContext>(options =>
            {
                options.UseSqlite(_contentConnection);
            });
            
            services.AddDbContext<StudentDbContext>(options =>
            {
                options.UseSqlite(_studentConnection);
            });
            
            services.AddDbContext<PaymentDbContext>(options =>
            {
                options.UseSqlite(_paymentConnection);
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_identityConnection);
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;
        
        var contentDbContext = services.GetRequiredService<ContentDbContext>();
        var studentDbContext = services.GetRequiredService<StudentDbContext>();
        var paymentDbContext = services.GetRequiredService<PaymentDbContext>();
        var identityDbContext = services.GetRequiredService<ApplicationDbContext>();
        
        await contentDbContext.Database.EnsureCreatedAsync();
        await studentDbContext.Database.EnsureCreatedAsync();
        await paymentDbContext.Database.EnsureCreatedAsync();
        await identityDbContext.Database.EnsureCreatedAsync();
        
        // Configurar Identity e criar roles básicos
        await ConfigureIdentityAsync(services);
    }

    private async Task ConfigureIdentityAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Criar roles
        string[] roles = { "Administrator", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
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
            _contentConnection?.Dispose();
            _studentConnection?.Dispose();
            _paymentConnection?.Dispose();
            _identityConnection?.Dispose();
        }

        base.Dispose(disposing);
    }
}