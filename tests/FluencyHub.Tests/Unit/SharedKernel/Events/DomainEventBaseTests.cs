using FluencyHub.SharedKernel.Events;
using FluentAssertions;
using Xunit;

namespace FluencyHub.Tests.Unit.SharedKernel.Events;

public class DomainEventBaseTests
{
    private class TestDomainEvent : DomainEventBase
    {
        public string Message { get; }
        
        public TestDomainEvent(string message = "Test Event")
        {
            Message = message;
        }
    }

    [Fact]
    public void DomainEventBase_Constructor_ShouldSetEventIdAndOccurredOn()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var domainEvent = new TestDomainEvent();
        var afterCreation = DateTime.UtcNow;
        
        // Assert
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredOn.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void DomainEventBase_EventId_ShouldBeUnique()
    {
        // Act
        var event1 = new TestDomainEvent("Event 1");
        var event2 = new TestDomainEvent("Event 2");
        var event3 = new TestDomainEvent("Event 3");
        var event4 = new TestDomainEvent("Event 4");
        var event5 = new TestDomainEvent("Event 5");
        
        // Assert
        var eventIds = new[] { event1.EventId, event2.EventId, event3.EventId, event4.EventId, event5.EventId };
        eventIds.Should().OnlyHaveUniqueItems();
        
        // Verificações individuais para garantir que cada ID é único
        event1.EventId.Should().NotBe(event2.EventId);
        event1.EventId.Should().NotBe(event3.EventId);
        event1.EventId.Should().NotBe(event4.EventId);
        event1.EventId.Should().NotBe(event5.EventId);
        event2.EventId.Should().NotBe(event3.EventId);
        event2.EventId.Should().NotBe(event4.EventId);
        event2.EventId.Should().NotBe(event5.EventId);
        event3.EventId.Should().NotBe(event4.EventId);
        event3.EventId.Should().NotBe(event5.EventId);
        event4.EventId.Should().NotBe(event5.EventId);
    }

    [Fact]
    public void DomainEventBase_OccurredOn_ShouldBeSetToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var domainEvent = new TestDomainEvent();
        var afterCreation = DateTime.UtcNow;
        
        // Assert
        domainEvent.OccurredOn.Should().BeOnOrAfter(beforeCreation);
        domainEvent.OccurredOn.Should().BeOnOrBefore(afterCreation);
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DomainEventBase_Constructor_ShouldGenerateValidGuid()
    {
        // Act
        var domainEvent = new TestDomainEvent();
        
        // Assert
        domainEvent.EventId.Should().NotBe(Guid.Empty);
        Guid.TryParse(domainEvent.EventId.ToString(), out var parsedGuid).Should().BeTrue();
        parsedGuid.Should().Be(domainEvent.EventId);
    }

    [Fact]
    public void DomainEventBase_ShouldImplementIDomainEvent()
    {
        // Act
        var domainEvent = new TestDomainEvent();
        
        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
        domainEvent.Should().BeAssignableTo<DomainEventBase>();
    }

    [Fact]
    public void DomainEventBase_MultipleInstances_ShouldHaveUniqueEventIds()
    {
        // Arrange
        var events = new List<TestDomainEvent>();
        
        // Act
        for (int i = 0; i < 100; i++)
        {
            events.Add(new TestDomainEvent($"Event {i}"));
        }
        
        // Assert
        var eventIds = events.Select(e => e.EventId).ToList();
        eventIds.Should().OnlyHaveUniqueItems();
        eventIds.Should().HaveCount(100);
    }

    [Fact]
    public void DomainEventBase_OccurredOn_ShouldBeInUtc()
    {
        // Act
        var domainEvent = new TestDomainEvent();
        
        // Assert
        // Verifica se o timestamp está próximo do UTC atual
        var utcNow = DateTime.UtcNow;
        domainEvent.OccurredOn.Should().BeCloseTo(utcNow, TimeSpan.FromSeconds(1));
        
        // Verifica se a diferença entre OccurredOn e UtcNow é pequena (confirmando que está em UTC)
        var timeDifference = Math.Abs((domainEvent.OccurredOn - utcNow).TotalMilliseconds);
        timeDifference.Should().BeLessThan(1000); // Menos de 1 segundo de diferença
    }

    [Fact]
    public void DomainEventBase_PropertiesAreReadOnly()
    {
        // Act
        var domainEvent = new TestDomainEvent();
        var eventId = domainEvent.EventId;
        var occurredOn = domainEvent.OccurredOn;
        
        // Assert
        // Aguarda um pouco e cria outro evento para verificar que os valores não mudam
        Thread.Sleep(1);
        
        domainEvent.EventId.Should().Be(eventId);
        domainEvent.OccurredOn.Should().Be(occurredOn);
    }

    [Fact]
    public void DomainEventBase_ConcurrentCreation_ShouldHaveUniqueIds()
    {
        // Arrange
        var events = new List<TestDomainEvent>();
        var lockObject = new object();
        
        // Act
        Parallel.For(0, 50, i =>
        {
            var domainEvent = new TestDomainEvent($"Concurrent Event {i}");
            lock (lockObject)
            {
                events.Add(domainEvent);
            }
        });
        
        // Assert
        events.Should().HaveCount(50);
        var eventIds = events.Select(e => e.EventId).ToList();
        eventIds.Should().OnlyHaveUniqueItems();
    }
} 