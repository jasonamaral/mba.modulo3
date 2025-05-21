using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Infrastructure.Services;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace FluencyHub.StudentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentManagementInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar DbContext com sua própria string de conexão
        services.AddDbContext<StudentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("StudentManagementConnection") 
                ?? throw new InvalidOperationException("StudentManagement connection string não configurada");
                
            options.UseSqlite(connectionString);
        });

        // Adicionar cache de memória para o serviço de eventos
        services.AddMemoryCache();

        // Adicionar Repositórios Student Management
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IStudentManagementEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<ILearningRepository, LearningRepository>();
        
        // Substituir o adaptador pelo serviço baseado em eventos
        services.AddScoped<ICourseRepository, CourseEventConsumerService>();

        return services;
    }
} 