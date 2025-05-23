using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Infrastructure.Services;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
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

        // Registrar Repositórios
        services.AddScoped<IApplicationStudentRepository, StudentRepository>();
        services.AddScoped<IDomainStudentRepository, StudentRepository>();
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

        return services;
    }
} 