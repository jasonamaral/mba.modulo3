using FluencyHub.Application.PaymentProcessing.Commands.ProcessPayment;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class PaymentProcessRequest
{
    [Required(ErrorMessage = "O ID da matr�cula � obrigat�rio.")]
    public Guid EnrollmentId { get; set; }

    [Required(ErrorMessage = "Os dados do cart�o s�o obrigat�rios.")]
    public CardDetailsRequest CardDetails { get; set; } = null!;

    [Required(ErrorMessage = "O valor � obrigat�rio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; set; }

    public ProcessPaymentCommand ToCommand()
    {
        return new ProcessPaymentCommand
        {
            EnrollmentId = EnrollmentId,
            CardHolderName = CardDetails.CardholderName,
            CardNumber = CardDetails.CardNumber,
            ExpiryMonth = CardDetails.ExpiryMonth.ToString(),
            ExpiryYear = CardDetails.ExpiryYear.ToString()
        };
    }
}
