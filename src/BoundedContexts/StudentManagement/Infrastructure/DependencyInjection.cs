using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Infrastructure.Services;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;
using FluencyHub.StudentManagement.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using FluencyHub.StudentManagement.Domain;
using IApplicationStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IDomainStudentRepository = FluencyHub.StudentManagement.Domain.IStudentRepository;
using FluencyHub.SharedKernel.Contracts;

namespace FluencyHub.StudentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentManagementInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar DbContext
        services.AddDbContext<StudentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("StudentManagementConnection")
                ?? throw new InvalidOperationException("StudentManagement connection string não configurada");
                
            options.UseSqlite(connectionString);
        });

        // Registrar ApplicationDbContext para Identity
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("IdentityConnection")
                ?? throw new InvalidOperationException("Identity connection string não configurada");
                
            options.UseSqlite(connectionString);
        });

        // Configurar Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Configurações de senha
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;

            // Configurações de usuário
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Registrar Repositórios
        services.AddScoped<IApplicationStudentRepository, StudentRepository>();
        services.AddScoped<IDomainStudentRepository, StudentRepository>();
        services.AddScoped<FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository, StudentRepository>();
        services.AddScoped<Application.Common.Interfaces.IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<ILearningRepository, LearningRepository>();

        // Adicionar cache de memória para o serviço de eventos
        services.AddMemoryCache();

        // Adicionar Repositórios Student Management
        services.AddScoped<IStudentManagementEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ILearningRepository, LearningRepository>();
        
        // Adicionar o repositório compartilhado para o EnrollmentRepository
        services.AddScoped<FluencyHub.SharedKernel.Contracts.IEnrollmentRepository, EnrollmentRepository>();
        
        // Substituir o adaptador pelo serviço baseado em eventos
        services.AddScoped<ICourseRepository, CourseEventConsumerService>();

        // Registrar o serviço de identidade
        services.AddScoped<IIdentityService, Services.IdentityService>();

        return services;
    }
} 