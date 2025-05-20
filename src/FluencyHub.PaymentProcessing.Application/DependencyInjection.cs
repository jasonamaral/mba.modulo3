using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Application.Services;

namespace FluencyHub.PaymentProcessing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentProcessingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        // Serviços de aplicação
        services.AddScoped<IPaymentApplicationService, PaymentService>();
        
        return services;
    }
} 