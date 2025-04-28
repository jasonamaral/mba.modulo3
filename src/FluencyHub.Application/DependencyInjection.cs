using System.Reflection;
using FluentValidation;
using FluencyHub.Application.Common.Behaviors;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.PaymentProcessing.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FluencyHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        
        // Serviços de aplicação
        services.AddScoped<IPaymentApplicationService, PaymentService>();

        return services;
    }
} 