using System;
using System.Threading;
using System.Threading.Tasks;
using FluencyHub.PaymentProcessing.Application.Common.Exceptions;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using FluencyHub.PaymentProcessing.Domain;
using Microsoft.Extensions.Logging;
using FluencyHub.SharedKernel.Events;
using FluencyHub.PaymentProcessing.Application.Common.Models;
using IPaymentRepository = FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentRepository;
using IPaymentGateway = FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway;
using IDomainEventService = FluencyHub.SharedKernel.Events.IDomainEventService;
using DomainCardDetails = FluencyHub.PaymentProcessing.Domain.CardDetails;
using ApplicationCardDetails = FluencyHub.PaymentProcessing.Application.Common.Models.CardDetails;

namespace FluencyHub.PaymentProcessing.Application.Services;

public class PaymentService : IPaymentApplicationService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IDomainEventService _eventService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IEnrollmentRepository enrollmentRepository,
        IPaymentGateway paymentGateway,
        IDomainEventService eventService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _enrollmentRepository = enrollmentRepository;
        _paymentGateway = paymentGateway;
        _eventService = eventService;
        _logger = logger;
    }

    public async Task<Guid> ProcessPaymentAsync(
        Guid studentId,
        string cardNumber,
        string cardHolderName,
        string expiryDate,
        string cvv,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cardDetails = new DomainCardDetails(
                cardHolderName,
                cardNumber,
                expiryDate.Split('/')[0],
                expiryDate.Split('/')[1]
            )
            {
                CardHolderName = cardHolderName,
                MaskedCardNumber = cardNumber.Substring(Math.Max(0, cardNumber.Length - 4)).PadLeft(cardNumber.Length, '*'),
                ExpiryMonth = expiryDate.Split('/')[0],
                ExpiryYear = expiryDate.Split('/')[1]
            };

            var payment = new Payment(studentId, Guid.NewGuid(), 0m, cardDetails)
            {
                CardDetails = cardDetails
            };

            var applicationCardDetails = new ApplicationCardDetails
            {
                CardHolderName = cardHolderName,
                CardNumber = cardNumber,
                MaskedCardNumber = cardDetails.MaskedCardNumber,
                ExpiryMonth = cardDetails.ExpiryMonth,
                ExpiryYear = cardDetails.ExpiryYear,
                Cvv = cvv
            };

            var gatewayResult = await _paymentGateway.ProcessPaymentAsync(
                payment.Id.ToString(),
                payment.Amount,
                applicationCardDetails);

            if (!gatewayResult.IsSuccessful)
                throw new PaymentProcessingException(gatewayResult.ErrorMessage ?? "Erro no processamento do pagamento");

            payment.MarkAsSuccess(gatewayResult.TransactionId!);

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            return payment.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento para estudante {StudentId}", studentId);
            throw;
        }
    }

    public async Task<Payment> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
            throw new PaymentNotFoundException(id);

        return payment;
    }

    public async Task RefundPaymentAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                throw new PaymentNotFoundException(paymentId);

            if (payment.Status != Domain.StatusPagamento.Aprovado)
                throw new InvalidPaymentStatusException("Pagamento não está em estado válido para reembolso");

            var refundResult = await _paymentGateway.ProcessRefundAsync(payment.TransactionId!, payment.Amount, reason);
            if (!refundResult.IsSuccessful)
                throw new PaymentProcessingException(refundResult.ErrorMessage ?? "Erro ao processar reembolso");

            payment.MarkAsRefunded(reason);
            await _paymentRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar reembolso para pagamento {PaymentId}", paymentId);
            throw;
        }
    }
} 