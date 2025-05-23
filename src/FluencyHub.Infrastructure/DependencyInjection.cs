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
        // Add Persistence
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var identityConnectionString = configuration.GetConnectionString("IdentityConnection");

        // Check environment to decide which database provider to use
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var usesSqlite = environment == "Development" || environment == "Testing";

        if (usesSqlite)
        {
            // Configure SQLite for Development and Testing environments
            services.AddDbContext<FluencyHubDbContext>(options =>
                options.UseSqlite(connectionString));
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(identityConnectionString));
        }
        else
        {
            // Configure SQL Server for Production environment
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

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Add Authentication with JWT
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

        // Add Identity Services
        services.AddScoped<Application.Common.Interfaces.IIdentityService, IdentityService>();

        // Add Payment Services
        services.AddHttpClient<Application.Common.Interfaces.IPaymentService, CieloPaymentService>(client =>
        {
            string baseUrl = configuration["PaymentGateway:BaseUrl"] ?? "https://api.cieloecommerce.cielo.com.br/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Add Payment Gateway
        services.AddScoped<Application.Common.Interfaces.IPaymentGateway, MockPaymentGateway>();

        return services;
    }
}