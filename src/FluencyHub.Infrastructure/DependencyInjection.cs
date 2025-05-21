using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Infrastructure.Identity;
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
        // Obter connection string para identidade
        var identityConnectionString = configuration.GetConnectionString("IdentityConnection") 
            ?? throw new InvalidOperationException("Identity connection string não configurada");

        // Adicionar DbContext de Identidade
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(identityConnectionString));
        
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
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}