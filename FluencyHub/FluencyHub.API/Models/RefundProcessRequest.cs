using System.ComponentModel.DataAnnotations;
using FluencyHub.Application.PaymentProcessing.Commands.RefundPayment;

namespace FluencyHub.API.Models;

public class RefundProcessRequest
{
    [Required]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 500 characters")]
    public string Reason { get; set; } = string.Empty;
    
    public RefundPaymentCommand ToCommand(Guid paymentId)
    {
        return new RefundPaymentCommand 
        { 
            PaymentId = paymentId,
            Reason = Reason
        };
    }
} 