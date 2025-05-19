using FluencyHub.Application.Common.Models;
using FluencyHub.PaymentProcessing.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FluencyHub.Infrastructure.Services;

public class CieloPaymentService : FluencyHub.Application.Common.Interfaces.IPaymentService
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

        // Configure HTTP client
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("MerchantId", _merchantId);
        _httpClient.DefaultRequestHeaders.Add("MerchantKey", _merchantKey);
    }

    public async Task<FluencyHub.Application.Common.Models.PaymentResult> ProcessPaymentAsync(
        Guid studentId,
        Guid enrollmentId,
        decimal amount,
        CardDetails cardDetails)
    {
        try
        {
            _logger.LogInformation("Processing payment for student {StudentId}, enrollment {EnrollmentId}",
                studentId, enrollmentId);

            // Create payment request object
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
                    Amount = (int)(amount * 100), // Cielo expects amount in cents
                    Installments = 1,
                    SoftDescriptor = "FluencyHub",
                    CreditCard = new
                    {
                        CardNumber = GetCardNumberFromMasked(cardDetails.MaskedCardNumber),
                        Holder = cardDetails.CardHolderName,
                        ExpirationDate = $"{cardDetails.ExpiryMonth}/{cardDetails.ExpiryYear}",
                        SecurityCode = "123", // In a real scenario, this would be provided by the customer
                        Brand = "Visa" // In a real scenario, this would be determined from the card number
                    },
                    Capture = true, // Automatically capture the payment
                    Authenticate = false
                }
            };

            // Serialize request
            var content = new StringContent(
                JsonSerializer.Serialize(paymentRequest),
                Encoding.UTF8,
                "application/json");

            // Make API call
            var response = await _httpClient.PostAsync("1/sales", content);

            // Process response
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseString);

                var paymentNode = responseJson.RootElement.GetProperty("Payment");
                var status = paymentNode.GetProperty("Status").GetInt32();
                var transactionId = paymentNode.GetProperty("PaymentId").GetString();

                // Cielo status codes: 2 = Authorized, 1 = Pending
                if (status == 2 || status == 1)
                {
                    _logger.LogInformation("Payment successful for enrollment {EnrollmentId}, transaction ID: {TransactionId}",
                        enrollmentId, transactionId);
                    return FluencyHub.Application.Common.Models.PaymentResult.Success(transactionId);
                }
                else
                {
                    var message = paymentNode.TryGetProperty("ReturnMessage", out var returnMessage)
                        ? returnMessage.GetString()
                        : "Payment failed with status code: " + status;

                    _logger.LogWarning("Payment failed for enrollment {EnrollmentId}: {Message}",
                        enrollmentId, message);
                    return FluencyHub.Application.Common.Models.PaymentResult.Failure(message);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Payment gateway returned error: {StatusCode}, {Error}",
                    response.StatusCode, errorContent);
                return FluencyHub.Application.Common.Models.PaymentResult.Failure($"Payment gateway error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while processing payment for enrollment {EnrollmentId}",
                enrollmentId);
            return FluencyHub.Application.Common.Models.PaymentResult.Failure($"Payment processing error: {ex.Message}");
        }
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId)
    {
        try
        {
            _logger.LogInformation("Checking payment status for transaction {TransactionId}", transactionId);

            // Make API call
            var response = await _httpClient.GetAsync($"1/sales/{transactionId}");

            // Process response
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseString);

                var paymentNode = responseJson.RootElement.GetProperty("Payment");
                var status = paymentNode.GetProperty("Status").GetInt32();

                // Map Cielo status to our status
                var paymentStatus = MapCieloStatusToPaymentStatus(status);
                var message = paymentNode.TryGetProperty("ReturnMessage", out var returnMessage)
                    ? returnMessage.GetString()
                    : null;

                _logger.LogInformation("Payment status for transaction {TransactionId}: {Status}",
                    transactionId, paymentStatus);
                return PaymentStatusResult.Create(transactionId, paymentStatus, message);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Payment gateway returned error when checking status: {StatusCode}, {Error}",
                    response.StatusCode, errorContent);
                return PaymentStatusResult.Create(transactionId, FluencyHub.Application.Common.Models.PaymentStatus.Unknown,
                    $"Failed to get status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking payment status for transaction {TransactionId}",
                transactionId);
            return PaymentStatusResult.Create(transactionId, FluencyHub.Application.Common.Models.PaymentStatus.Unknown,
                $"Error checking status: {ex.Message}");
        }
    }

    public async Task<FluencyHub.Application.Common.Models.RefundResult> RequestRefundAsync(string transactionId, decimal amount)
    {
        try
        {
            _logger.LogInformation("Requesting refund for transaction {TransactionId}, amount {Amount}",
                transactionId, amount);

            // Create refund request object
            var refundRequest = new
            {
                Amount = (int)(amount * 100) // Cielo expects amount in cents
            };

            // Serialize request
            var content = new StringContent(
                JsonSerializer.Serialize(refundRequest),
                Encoding.UTF8,
                "application/json");

            // Make API call
            var response = await _httpClient.PutAsync($"1/sales/{transactionId}/void", content);

            // Process response
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseString);

                var status = responseJson.RootElement.GetProperty("Status").GetInt32();

                // Cielo status code for successful refund is 10 or 11
                if (status == 10 || status == 11)
                {
                    var refundTransactionId = responseJson.RootElement.TryGetProperty("VoidId", out var voidId)
                        ? voidId.GetString()
                        : Guid.NewGuid().ToString();

                    _logger.LogInformation("Refund successful for transaction {TransactionId}, refund ID: {RefundId}",
                        transactionId, refundTransactionId);
                    return FluencyHub.Application.Common.Models.RefundResult.Success(transactionId, refundTransactionId, amount);
                }
                else
                {
                    var message = responseJson.RootElement.TryGetProperty("ReturnMessage", out var returnMessage)
                        ? returnMessage.GetString()
                        : "Refund failed with status code: " + status;

                    _logger.LogWarning("Refund failed for transaction {TransactionId}: {Message}",
                        transactionId, message);
                    return FluencyHub.Application.Common.Models.RefundResult.Failure(transactionId, message);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Payment gateway returned error when requesting refund: {StatusCode}, {Error}",
                    response.StatusCode, errorContent);
                return FluencyHub.Application.Common.Models.RefundResult.Failure(transactionId, $"Refund request error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while requesting refund for transaction {TransactionId}",
                transactionId);
            return FluencyHub.Application.Common.Models.RefundResult.Failure(transactionId, $"Refund processing error: {ex.Message}");
        }
    }

    private FluencyHub.Application.Common.Models.PaymentStatus MapCieloStatusToPaymentStatus(int cieloStatus)
    {
        return cieloStatus switch
        {
            1 => FluencyHub.Application.Common.Models.PaymentStatus.Pending,          // Pending
            2 => FluencyHub.Application.Common.Models.PaymentStatus.Authorized,       // Authorized
            3 => FluencyHub.Application.Common.Models.PaymentStatus.Cancelled,        // Denied
            4 => FluencyHub.Application.Common.Models.PaymentStatus.Completed,        // Paid
            10 => FluencyHub.Application.Common.Models.PaymentStatus.Refunded,        // Refunded
            11 => FluencyHub.Application.Common.Models.PaymentStatus.Refunded,        // Partially Refunded
            13 => FluencyHub.Application.Common.Models.PaymentStatus.Cancelled,       // Expired
            _ => FluencyHub.Application.Common.Models.PaymentStatus.Unknown           // Other statuses
        };
    }

    private string GetCardNumberFromMasked(string maskedNumber)
    {
        // In a real implementation, we would never use the actual card number
        // This is just for demonstration purposes
        if (maskedNumber.Contains("****"))
        {
            // Replace masked portion with dummy numbers for demo
            return maskedNumber.Replace("****", "1111");
        }

        return maskedNumber;
    }
}