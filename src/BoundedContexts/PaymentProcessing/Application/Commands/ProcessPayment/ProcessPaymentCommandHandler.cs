using MediatR;
using Microsoft.Extensions.Logging;
using FluencyHub.PaymentProcessing.Application.Common.Interfaces;
using AppModels = FluencyHub.PaymentProcessing.Application.Common.Models;
using FluencyHub.PaymentProcessing.Domain;
using AppInterfaces = FluencyHub.PaymentProcessing.Application.Common.Interfaces;

namespace FluencyHub.PaymentProcessing.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly AppInterfaces.IPaymentRepository _paymentRepository;
    private readonly AppInterfaces.IPaymentGateway _paymentGateway;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        AppInterfaces.IPaymentRepository paymentRepository,
        AppInterfaces.IPaymentGateway paymentGateway,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {


        try
        {
            // Criar detalhes do cartão para o gateway (modelo da aplicação)
            var gatewayCardDetails = new AppModels.CardDetails
            {
                CardHolderName = request.CardHolderName,
                CardNumber = request.CardNumber,
                MaskedCardNumber = MaskCardNumber(request.CardNumber),
                ExpiryMonth = request.ExpirationDate.Split('/')[0],
                ExpiryYear = request.ExpirationDate.Split('/')[1],
                Cvv = request.SecurityCode
            };

            // Processar pagamento através do gateway
            var orderId = Guid.NewGuid().ToString();
            var paymentResult = await _paymentGateway.ProcessPaymentAsync(
                orderId,
                request.Amount,
                gatewayCardDetails);

            // Criar detalhes do cartão para o domínio
            var domainCardDetails = new CardDetails(
                request.CardHolderName,
                request.CardNumber,
                request.ExpirationDate.Split('/')[0],
                request.ExpirationDate.Split('/')[1]);

            // Criar entidade de pagamento
            var payment = new Payment(
                request.StudentId,
                request.EnrollmentId,
                request.Amount,
                domainCardDetails)
            {
                CardDetails = domainCardDetails
            };

            // Marcar como sucesso ou falha baseado no resultado do gateway
            if (paymentResult.IsSuccessful && !string.IsNullOrEmpty(paymentResult.TransactionId))
            {
                payment.MarkAsSuccess(paymentResult.TransactionId);
            }
            else
            {
                payment.MarkAsFailed(paymentResult.ErrorMessage ?? "Falha no processamento do pagamento");
            }

            // Salvar no repositório
            await _paymentRepository.AddAsync(payment);



            return payment.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento para estudante {StudentId}", request.StudentId);
            throw;
        }
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
            return cardNumber;

        return "**** **** **** " + cardNumber.Substring(cardNumber.Length - 4);
    }
} 