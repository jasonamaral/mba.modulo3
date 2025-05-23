namespace FluencyHub.PaymentProcessing.Application.Common.Models;

public class CardDetails
{
    public required string CardHolderName { get; set; }
    public required string CardNumber { get; set; }
    public required string MaskedCardNumber { get; set; }
    public required string ExpiryMonth { get; set; }
    public required string ExpiryYear { get; set; }
    public required string Cvv { get; set; }
} 