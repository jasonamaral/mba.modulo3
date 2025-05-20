using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using FluencyHub.API.Adapters;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence.Repositories;
using FluencyHub.PaymentProcessing.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluencyHub.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
    
    public static IServiceCollection AddRepositoryAdapters(this IServiceCollection services)
    {
        // Registrar os repositórios base
        services.AddScoped<CourseRepository>();
        services.AddScoped<StudentRepository>();
        services.AddScoped<EnrollmentRepository>();
        services.AddScoped<CertificateRepository>();
        services.AddScoped<PaymentRepository>();
        services.AddScoped<LearningRepository>();
        services.AddScoped<LessonRepository>();
        services.AddScoped<MockPaymentGateway>();
        
        // Registrar os adaptadores
        services.AddScoped<ICourseRepository, CourseRepositoryAdapter>();
        services.AddScoped<IStudentRepository, StudentRepositoryAdapter>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepositoryAdapter>();
        services.AddScoped<ICertificateRepository, CertificateRepositoryAdapter>();
        services.AddScoped<IPaymentRepository, PaymentRepositoryAdapter>();
        services.AddScoped<ILessonRepository, LessonRepositoryAdapter>();
        services.AddScoped<ILearningRepository, LearningRepositoryAdapter>();
        services.AddScoped<IPaymentGateway, PaymentGatewayAdapter>();
        
        // Registrar o adaptador específico para o PaymentProcessing
        services.AddScoped<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IEnrollmentRepository, PaymentProcessingEnrollmentRepositoryAdapter>();
        
        return services;
    }
}