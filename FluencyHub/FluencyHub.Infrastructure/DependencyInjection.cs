using System.Text;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.PaymentProcessing;
using FluencyHub.Domain.StudentManagement;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using FluencyHub.Infrastructure.Persistence.Repositories;
using FluencyHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FluencyHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Persistence
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<FluencyHubDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<Application.Common.Interfaces.ICourseRepository, CourseRepository>();
        services.AddScoped<Application.Common.Interfaces.IStudentRepository, StudentRepository>();
        services.AddScoped<Application.Common.Interfaces.IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<Application.Common.Interfaces.ICertificateRepository, CertificateRepository>();
        services.AddScoped<Application.Common.Interfaces.IPaymentRepository, PaymentRepository>();

        // Add Identity
        var identityConnectionString = configuration.GetConnectionString("IdentityConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(identityConnectionString));

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
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"] ?? "")),
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