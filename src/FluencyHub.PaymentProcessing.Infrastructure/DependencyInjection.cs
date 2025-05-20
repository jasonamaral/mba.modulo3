using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
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
        // Adicionar Repositórios Payment Processing
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        // Adicionar Serviços de Pagamento
        services.AddHttpClient<IPaymentService, CieloPaymentService>((serviceProvider, client) =>
        {
            string baseUrl = configuration["PaymentGateway:BaseUrl"] ?? "https://api.cieloecommerce.cielo.com.br/";
            client.BaseAddress = new Uri(baseUrl);
        });

        // Registrar o serviço de pagamento com as dependências necessárias
        services.AddScoped<IPaymentService>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(CieloPaymentService));
            var logger = provider.GetRequiredService<ILogger<CieloPaymentService>>();
            return new CieloPaymentService(httpClient, logger, configuration);
        });

        // Adicionar Gateway de Pagamento
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();

        return services;
    }
} 