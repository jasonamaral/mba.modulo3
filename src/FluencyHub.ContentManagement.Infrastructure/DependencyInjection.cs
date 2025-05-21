using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.ContentManagement.Application.Commands.CompleteEnrollment;

namespace FluencyHub.ContentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContentManagementInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar DbContext com sua própria string de conexão
        services.AddDbContext<ContentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("ContentManagementConnection") 
                ?? throw new InvalidOperationException("ContentManagement connection string não configurada");
                
            options.UseSqlite(connectionString);
        });

        // Registrar explicitamente como Domain.ICourseRepository e Application.Common.Interfaces.ICourseRepository
        // para resolver o conflito de dependências
        services.AddScoped<Domain.ICourseRepository, Persistence.Repositories.CourseRepository>();
        services.AddScoped<Application.Common.Interfaces.ICourseRepository, Persistence.Repositories.CourseRepository>();
        services.AddScoped<Application.Common.Interfaces.ILessonRepository, Persistence.Repositories.LessonRepository>();
        
        // Adicionar adaptador para o EnrollmentRepository usado por CompleteEnrollmentCommand
        services.AddScoped<IEnrollmentRepository, EnrollmentRepositoryAdapter>();

        return services;
    }
} 