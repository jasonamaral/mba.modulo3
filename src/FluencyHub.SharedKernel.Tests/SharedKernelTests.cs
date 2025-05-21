using FluencyHub.SharedKernel.Events;
using Xunit;
using System;

namespace FluencyHub.SharedKernel.Tests;

public class SharedKernelTests
{
    [Fact]
    public void DomainEvent_CanBeCreated()
    {
        // Arrange & Act
        var testEvent = new TestDomainEvent(Guid.NewGuid());
        
        // Assert
        Assert.NotEqual(Guid.Empty, testEvent.EventId);
        Assert.True(testEvent.OccurredOn > DateTime.MinValue);
    }
    
    private class TestDomainEvent : DomainEventBase
    {
        public Guid EntityId { get; }
        
        public TestDomainEvent(Guid entityId)
        {
            EntityId = entityId;
        }
    }
} 