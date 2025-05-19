using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using FluencyHub.Infrastructure.Persistence.Repositories;
using FluencyHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FluencyHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Adicionar Persistência
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var identityConnectionString = configuration.GetConnectionString("IdentityConnection");

        // Verificar o ambiente para decidir qual provedor de banco de dados usar
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var usesSqlite = environment == "Development" || environment == "Testing";

        if (usesSqlite)
        {
            // Configurar SQLite para ambientes de Desenvolvimento e Teste
            services.AddDbContext<FluencyHubDbContext>(options =>
                options.UseSqlite(connectionString));
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(identityConnectionString));
        }
        else
        {
            // Configurar SQL Server para o ambiente de Produção
            services.AddDbContext<FluencyHubDbContext>(options =>
                options.UseSqlServer(connectionString));
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(identityConnectionString));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<FluencyHubDbContext>());

        services.AddScoped<Application.Common.Interfaces.ICourseRepository, CourseRepository>();
        services.AddScoped<Application.Common.Interfaces.IStudentRepository, StudentRepository>();
        services.AddScoped<Application.Common.Interfaces.IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<Application.Common.Interfaces.ICertificateRepository, CertificateRepository>();
        services.AddScoped<Application.Common.Interfaces.IPaymentRepository, PaymentRepository>();
        services.AddScoped<Application.Common.Interfaces.ILearningRepository, LearningRepository>();
        services.AddScoped<Application.Common.Interfaces.ILessonRepository, LessonRepository>();
        services.AddScoped<Application.Common.Interfaces.IDomainEventService, DomainEventService>();

        // Adicionar Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Configurações de senha
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Configurações de bloqueio
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // Configurações de usuário
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Adicionar Autenticação com JWT
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                
                var jwtSecret = configuration["JwtSettings:Secret"] 
                    ?? throw new InvalidOperationException("JWT Secret is not configured");
                var jwtIssuer = configuration["JwtSettings:Issuer"] 
                    ?? throw new InvalidOperationException("JWT Issuer is not configured");
                var jwtAudience = configuration["JwtSettings:Audience"] 
                    ?? throw new InvalidOperationException("JWT Audience is not configured");
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Adicionar Serviços de Identity
        services.AddScoped<Application.Common.Interfaces.IIdentityService, IdentityService>();

        // Adicionar Serviços de Pagamento
        services.AddHttpClient<Application.Common.Interfaces.IPaymentService, CieloPaymentService>(client =>
        {
            string baseUrl = configuration["PaymentGateway:BaseUrl"] ?? "https://api.cieloecommerce.cielo.com.br/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Adicionar Gateway de Pagamento
        services.AddScoped<Application.Common.Interfaces.IPaymentGateway, MockPaymentGateway>();

        return services;
    }
}