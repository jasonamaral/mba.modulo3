using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluencyHub.ContentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddContentManagementInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Adicionar Repositórios Content Management
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();

        return services;
    }
} 