using FluencyHub.Application.PaymentProcessing.Commands.ProcessPayment;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class PaymentProcessRequest
{
    [Required(ErrorMessage = "Enrollment ID is required.")]
    public Guid EnrollmentId { get; set; }

    [Required(ErrorMessage = "Card details are required.")]
    public CardDetailsRequest CardDetails { get; set; } = null!;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
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
