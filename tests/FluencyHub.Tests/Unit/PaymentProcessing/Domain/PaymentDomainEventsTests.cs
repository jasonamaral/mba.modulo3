using FluentAssertions;
using FluencyHub.PaymentProcessing.Domain.Events;
using Xunit;

namespace FluencyHub.Tests.Unit.PaymentProcessing.Domain;

public class PaymentDomainEventsTests
{
    [Fact]
    public void PaymentConfirmedDomainEvent_Constructor_ShouldCreateEvent_WhenValidParameters()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var transactionId = "TXN123456789";

        // Act
        var domainEvent = new PaymentConfirmedDomainEvent(paymentId, enrollmentId, transactionId);

        // Assert
        domainEvent.PaymentId.Should().Be(paymentId);
        domainEvent.EnrollmentId.Should().Be(enrollmentId);
        domainEvent.TransactionId.Should().Be(transactionId);
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void PaymentRejectedDomainEvent_Constructor_ShouldCreateEvent_WhenValidParameters()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var failureReason = "Cartão recusado pela operadora";

        // Act
        var domainEvent = new PaymentRejectedDomainEvent(paymentId, enrollmentId, failureReason);

        // Assert
        domainEvent.PaymentId.Should().Be(paymentId);
        domainEvent.EnrollmentId.Should().Be(enrollmentId);
        domainEvent.FailureReason.Should().Be(failureReason);
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void PaymentConfirmedDomainEvent_EventId_ShouldBeUnique()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var transactionId = "TXN123456789";

        // Act
        var event1 = new PaymentConfirmedDomainEvent(paymentId, enrollmentId, transactionId);
        var event2 = new PaymentConfirmedDomainEvent(paymentId, enrollmentId, transactionId);

        // Assert
        event1.EventId.Should().NotBe(event2.EventId);
    }

    [Fact]
    public void PaymentRejectedDomainEvent_EventId_ShouldBeUnique()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var failureReason = "Cartão recusado";

        // Act
        var event1 = new PaymentRejectedDomainEvent(paymentId, enrollmentId, failureReason);
        var event2 = new PaymentRejectedDomainEvent(paymentId, enrollmentId, failureReason);

        // Assert
        event1.EventId.Should().NotBe(event2.EventId);
    }

    [Fact]
    public void PaymentConfirmedDomainEvent_OccurredOn_ShouldBeSetToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var paymentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var transactionId = "TXN123456789";

        // Act
        var domainEvent = new PaymentConfirmedDomainEvent(paymentId, enrollmentId, transactionId);
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredOn.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void PaymentRejectedDomainEvent_OccurredOn_ShouldBeSetToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var paymentId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();
        var failureReason = "Cartão recusado";

        // Act
        var domainEvent = new PaymentRejectedDomainEvent(paymentId, enrollmentId, failureReason);
        var afterCreation = DateTime.UtcNow;

        // Assert
        domainEvent.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredOn.Should().BeOnOrBefore(afterCreation);
    }
} 