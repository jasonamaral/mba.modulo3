using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence.Repositories;
using FluencyHub.PaymentProcessing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluencyHub.PaymentProcessing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentProcessingInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar DbContext com sua própria string de conexão
        services.AddDbContext<PaymentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("PaymentProcessingConnection") 
                ?? throw new InvalidOperationException("PaymentProcessing connection string não configurada");
                
            options.UseSqlite(connectionString);
        });

        // Adicionar Repositórios
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        // Adicionar EnrollmentRepository do Student Management (adaptador)
        services.AddScoped<IEnrollmentRepository, EnrollmentRepositoryAdapter>();

        // Adicionar Gateway de Pagamento
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();

        // Adicionar Serviços de Pagamento
        services.AddHttpClient<IPaymentService, CieloPaymentService>((serviceProvider, client) =>
        {
            client.BaseAddress = new Uri(configuration["PaymentGateway:BaseUrl"] ?? "");
        });

        // Registrar o serviço de pagamento com as dependências necessárias
        services.AddScoped<IPaymentService>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(CieloPaymentService));
            var logger = provider.GetRequiredService<ILogger<CieloPaymentService>>();
            return new CieloPaymentService(httpClient, logger, configuration);
        });

        return services;
    }
} 