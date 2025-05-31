using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Application.Common.Models;

namespace FluencyHub.PaymentProcessing.Infrastructure.Services;

public class CieloPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CieloPaymentService> _logger;
    private readonly string _merchantId;
    private readonly string _merchantKey;

    public CieloPaymentService(
        HttpClient httpClient,
        ILogger<CieloPaymentService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _merchantId = configuration["PaymentGateway:MerchantId"]
            ?? throw new ArgumentException("PaymentGateway:MerchantId configuration is missing");

        _merchantKey = configuration["PaymentGateway:MerchantKey"]
            ?? throw new ArgumentException("PaymentGateway:MerchantKey configuration is missing");

        // Configurar o cliente HTTP
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("MerchantId", _merchantId);
        _httpClient.DefaultRequestHeaders.Add("MerchantKey", _merchantKey);
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        Guid studentId,
        Guid enrollmentId,
        decimal amount,
        CardDetails cardDetails)
    {
        try
        {
            // Criar objeto de solicitação de pagamento
            var paymentRequest = new
            {
                MerchantOrderId = enrollmentId.ToString(),
                Customer = new
                {
                    Name = cardDetails.CardHolderName
                },
                Payment = new
                {
                    Type = "CreditCard",
                    Amount = (int)(amount * 100), // Cielo espera o valor em centavos
                    Installments = 1,
                    SoftDescriptor = "FluencyHub",
                    CreditCard = new
                    {
                        CardNumber = cardDetails.CardNumber,
                        Holder = cardDetails.CardHolderName,
                        ExpirationDate = $"{cardDetails.ExpiryMonth}/{cardDetails.ExpiryYear}",
                        SecurityCode = cardDetails.Cvv,
                        Brand = "Visa" // Em um cenário real, isso seria determinado pelo número do cartão
                    },
                    Capture = true, // Capturar o pagamento automaticamente
                    Authenticate = false
                }
            };

            // Serializar a requisição
            var content = new StringContent(
                JsonSerializer.Serialize(paymentRequest),
                Encoding.UTF8,
                "application/json");

            // Fazer a chamada à API
            var response = await _httpClient.PostAsync("1/sales", content);

            // Processar a resposta
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseString);

                var paymentNode = responseJson.RootElement.GetProperty("Payment");
                var status = paymentNode.GetProperty("Status").GetInt32();
                var transactionId = paymentNode.GetProperty("PaymentId").GetString();

                // Códigos de status da Cielo: 2 = Autorizado, 1 = Pendente
                if (status == 2 || status == 1)
                {
                    return PaymentResult.Success(transactionId ?? string.Empty);
                }
                else
                {
                    var message = paymentNode.TryGetProperty("ReturnMessage", out var returnMessage)
                        ? returnMessage.GetString()
                        : "Pagamento falhou com código de status: " + status;

                    return PaymentResult.Failure(message ?? "Erro desconhecido");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return PaymentResult.Failure($"Erro no gateway de pagamento: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while processing payment for enrollment {EnrollmentId}",
                enrollmentId);
            return PaymentResult.Failure($"Erro no processamento do pagamento: {ex.Message}");
        }
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId)
    {
        try
        {
            // Fazer chamada à API
            var response = await _httpClient.GetAsync($"1/sales/{transactionId}");

            // Processar a resposta
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseString);

                var paymentNode = responseJson.RootElement.GetProperty("Payment");
                var status = paymentNode.GetProperty("Status").GetInt32();

                // Mapear o status da Cielo para o nosso status
                var paymentStatus = MapCieloStatusToPaymentStatus(status);
                var message = paymentNode.TryGetProperty("ReturnMessage", out var returnMessage)
                    ? returnMessage.GetString()
                    : null;

                return PaymentStatusResult.Create(transactionId, paymentStatus, message);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return PaymentStatusResult.Create(transactionId, StatusPagamento.Desconhecido,
                    $"Falha ao obter status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking payment status for transaction {TransactionId}",
                transactionId);
            return PaymentStatusResult.Create(transactionId, StatusPagamento.Desconhecido,
                $"Erro ao verificar status: {ex.Message}");
        }
    }

    public async Task<RefundResult> RequestRefundAsync(string transactionId, decimal amount)
    {
        try
        {
            // Criar objeto de solicitação de reembolso
            var refundRequest = new
            {
                Amount = (int)(amount * 100) // Cielo espera o valor em centavos
            };

            // Serializar a requisição
            var content = new StringContent(
                JsonSerializer.Serialize(refundRequest),
                Encoding.UTF8,
                "application/json");

            // Fazer chamada à API
            var response = await _httpClient.PutAsync($"1/sales/{transactionId}/void", content);

            // Processar a resposta
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseString);

                var status = responseJson.RootElement.GetProperty("Status").GetInt32();

                // Código de status da Cielo para reembolso bem-sucedido é 10 ou 11
                if (status == 10 || status == 11)
                {
                    var refundTransactionId = responseJson.RootElement.TryGetProperty("VoidId", out var voidId)
                        ? voidId.GetString()
                        : Guid.NewGuid().ToString();

                    return RefundResult.Success(transactionId, refundTransactionId ?? string.Empty, amount);
                }
                else
                {
                    var message = responseJson.RootElement.TryGetProperty("ReturnMessage", out var returnMessage)
                        ? returnMessage.GetString()
                        : "Reembolso falhou com código de status: " + status;

                    return RefundResult.Failure(transactionId, message ?? "Erro desconhecido");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return RefundResult.Failure(transactionId, $"Erro na solicitação de reembolso: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while requesting refund for transaction {TransactionId}",
                transactionId);
            return RefundResult.Failure(transactionId, $"Erro no processamento do reembolso: {ex.Message}");
        }
    }

    private StatusPagamento MapCieloStatusToPaymentStatus(int cieloStatus)
    {
        return cieloStatus switch
        {
            1 => StatusPagamento.Pendente,
            2 => StatusPagamento.Autorizado,
            3 => StatusPagamento.Cancelado,
            10 => StatusPagamento.Reembolsado,
            11 => StatusPagamento.Reembolsado,
            12 => StatusPagamento.Pendente,
            13 => StatusPagamento.Falha,
            20 => StatusPagamento.Falha,
            _ => StatusPagamento.Desconhecido
        };
    }
} 