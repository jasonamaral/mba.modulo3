using FluencyHub.API.Models;
using FluencyHub.Application.PaymentProcessing.Queries.GetPaymentById;
using FluencyHub.PaymentProcessing.Domain;
using Swashbuckle.AspNetCore.Filters;
using System;

namespace FluencyHub.API.SwaggerExamples;

public class PaymentProcessRequestExample : IExamplesProvider<PaymentProcessRequest>
{
    public PaymentProcessRequest GetExamples()
    {
        return new PaymentProcessRequest
        {
            EnrollmentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Amount = 99.99m,
            CardDetails = new CardDetailsRequest
            {
                CardholderName = "John Doe",
                CardNumber = "4111111111111111",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Cvv = "123"
            }
        };
    }
}

public class RefundProcessRequestExample : IExamplesProvider<RefundProcessRequest>
{
    public RefundProcessRequest GetExamples()
    {
        return new RefundProcessRequest
        {
            Reason = "Customer requested refund"
        };
    }
}

public class PaymentDtoExample : IExamplesProvider<PaymentDto>
{
    public PaymentDto GetExamples()
    {
        return new PaymentDto
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            EnrollmentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Amount = 99.99m,
            Status = PaymentStatus.Successful.ToString(),
            TransactionId = "TRX123456789",
            FailureReason = null,
            RefundReason = null,
            CreatedAt = DateTime.Now.AddDays(-5),
            UpdatedAt = DateTime.Now.AddDays(-5)
        };
    }
} 