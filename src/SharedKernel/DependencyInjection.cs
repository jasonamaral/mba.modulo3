using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluencyHub.SharedKernel.Events;

namespace FluencyHub.SharedKernel;

public static class DependencyInjection
{
    /// <summary>
    /// Adiciona os serviços do SharedKernel ao contêiner de DI
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <returns>Coleção de serviços atualizada</returns>
    public static IServiceCollection AddSharedKernelServices(this IServiceCollection services)
    {
        // Registrar serviço de eventos de domínio
        services.AddScoped<IDomainEventService, DomainEventService>();
        
        // Registrar todos os handlers de eventos do SharedKernel
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        return services;
    }
    
    /// <summary>
    /// Adiciona os serviços do MediatR ao contêiner de DI
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="assemblies">Assemblies a serem escaneados por handlers</param>
    /// <returns>Coleção de serviços atualizada</returns>
    public static IServiceCollection AddMediatorServices(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
        });
        
        return services;
    }
} 