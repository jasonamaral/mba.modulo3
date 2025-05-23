using FluencyHub.PaymentProcessing.Domain;
using FluencyHub.PaymentProcessing.Application.Common.Models;

namespace FluencyHub.PaymentProcessing.Application.Common.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Processa um pagamento usando o gateway de pagamento
    /// </summary>
    /// <param name="studentId">ID do estudante que está realizando o pagamento</param>
    /// <param name="enrollmentId">ID da matrícula sendo paga</param>
    /// <param name="amount">Valor a ser cobrado</param>
    /// <param name="cardDetails">Detalhes do cartão de crédito</param>
    /// <returns>Resultado do processo de pagamento</returns>
    Task<FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult> ProcessPaymentAsync(
        Guid studentId,
        Guid enrollmentId,
        decimal amount,
        Models.CardDetails cardDetails);
    
    /// <summary>
    /// Obtém o status de pagamento do gateway de pagamento
    /// </summary>
    /// <param name="transactionId">ID da transação no gateway de pagamento</param>
    /// <returns>Status atual do pagamento</returns>
    Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId);
    
    /// <summary>
    /// Solicita um reembolso para um pagamento
    /// </summary>
    /// <param name="transactionId">ID da transação original</param>
    /// <param name="amount">Valor a ser reembolsado (pode ser parcial)</param>
    /// <returns>Resultado da solicitação de reembolso</returns>
    Task<RefundResult> RequestRefundAsync(string transactionId, decimal amount);
} 