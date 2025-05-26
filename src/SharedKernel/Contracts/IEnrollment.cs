using System;

namespace FluencyHub.SharedKernel.Contracts;

public interface IEnrollment
{
    Guid Id { get; }
    Guid StudentId { get; }
    Guid CourseId { get; }
    decimal Price { get; }
    string Status { get; }
    DateTime EnrollmentDate { get; }
    DateTime? ActivationDate { get; }
    DateTime? CompletionDate { get; }
    Guid? PaymentId { get; }
    string? PaymentFailureReason { get; }
} 