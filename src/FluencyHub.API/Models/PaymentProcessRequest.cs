using FluencyHub.PaymentProcessing.Application.Commands.ProcessPayment;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.API.Models;

public class PaymentProcessRequest
{
    [Required(ErrorMessage = "Enrollment ID is required.")]
    public Guid EnrollmentId { get; set; }

    [Required(ErrorMessage = "Student ID is required.")]
    public Guid StudentId { get; set; }

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
