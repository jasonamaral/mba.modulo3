using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluencyHub.StudentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentManagementInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Adicionar Repositórios Student Management
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<ILearningRepository, LearningRepository>();

        return services;
    }
} 