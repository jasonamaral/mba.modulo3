using FluencyHub.PaymentProcessing.Application.Commands.ProcessPayment;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class PaymentProcessRequest
{
    [Required(ErrorMessage = "ID da matrícula é obrigatório.")]
    public Guid EnrollmentId { get; set; }

    [Required(ErrorMessage = "ID do estudante é obrigatório.")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Detalhes do cartão são obrigatórios.")]
    public CardDetailsRequest CardDetails { get; set; } = null!;

    [Required(ErrorMessage = "Valor é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero.")]
    public decimal Amount { get; set; }

    public ProcessPaymentCommand ToCommand()
    {
        return new ProcessPaymentCommand
        {
            EnrollmentId = EnrollmentId,
            StudentId = StudentId,
            Amount = Amount,
            PaymentMethod = "CreditCard",
            CardHolderName = CardDetails.CardholderName,
            CardNumber = CardDetails.CardNumber,
            ExpirationDate = $"{CardDetails.ExpiryMonth:D2}/{CardDetails.ExpiryYear}",
            SecurityCode = CardDetails.Cvv
        };
    }
}
