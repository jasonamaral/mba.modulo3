using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Application.Handlers;
using FluencyHub.SharedKernel.Events.PaymentProcessing;

namespace FluencyHub.StudentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentManagementApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        services.AddScoped<INotificationHandler<PaymentProcessedEvent>, PaymentProcessedEventHandler>();
        services.AddScoped<INotificationHandler<EnrollmentActivatedEvent>, EnrollmentActivatedEventHandler>();
        
        return services;
    }
} 