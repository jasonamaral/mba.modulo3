using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using FluencyHub.ContentManagement.Application.Common.Behaviors;
using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Application.Handlers;
using FluencyHub.SharedKernel.Events.StudentManagement;

namespace FluencyHub.ContentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddContentManagementApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        services.AddScoped<INotificationHandler<StudentEnrolledEvent>, StudentEnrolledEventHandler>();
        services.AddScoped<INotificationHandler<LessonCompletedEvent>, LessonCompletedEventHandler>();
        
        return services;
    }
} 