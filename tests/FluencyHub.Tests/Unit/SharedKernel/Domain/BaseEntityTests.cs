using FluencyHub.SharedKernel.Domain;
using FluencyHub.SharedKernel.Events;
using FluentAssertions;
using Xunit;

namespace FluencyHub.Tests.Unit.SharedKernel.Domain;

public class BaseEntityTests
{
    // Classe concreta para testes da BaseEntity
    private class TestEntity : BaseEntity
    {
        public TestEntity() : base() { }
        
        public void AddTestDomainEvent(IDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }
    }
    
    // Implementação simples de IDomainEvent para testes
    private class TestDomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    [Fact]
    public void BaseEntity_Constructor_ShouldSetCreatedAndUpdatedDates()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var entity = new TestEntity();
        var afterCreation = DateTime.UtcNow;
        
        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        entity.CreatedAt.Should().BeOnOrBefore(afterCreation);
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void BaseEntity_Constructor_ShouldGenerateId()
    {
        // Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();
        
        // Assert
        entity1.Id.Should().NotBeEmpty();
        entity2.Id.Should().NotBeEmpty();
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void BaseEntity_AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        
        // Act
        entity.AddTestDomainEvent(domainEvent);
        
        // Assert
        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void BaseEntity_ClearDomainEvents_ShouldClearEventCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent1 = new TestDomainEvent();
        var domainEvent2 = new TestDomainEvent();
        
        entity.AddTestDomainEvent(domainEvent1);
        entity.AddTestDomainEvent(domainEvent2);
        
        // Act
        entity.ClearDomainEvents();
        
        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void BaseEntity_RemoveDomainEvent_ShouldRemoveSpecificEvent()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent1 = new TestDomainEvent();
        var domainEvent2 = new TestDomainEvent();
        
        entity.AddTestDomainEvent(domainEvent1);
        entity.AddTestDomainEvent(domainEvent2);
        
        // Act
        entity.RemoveDomainEvent(domainEvent1);
        
        // Assert
        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Should().NotContain(domainEvent1);
        entity.DomainEvents.Should().Contain(domainEvent2);
    }

    [Fact]
    public void BaseEntity_DomainEvents_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var entity = new TestEntity();
        
        // Act
        var domainEvents = entity.DomainEvents;
        
        // Assert
        domainEvents.Should().NotBeNull();
        domainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }

    [Fact]
    public void BaseEntity_Constructor_ShouldSetIsActiveToTrue()
    {
        // Act
        var entity = new TestEntity();
        
        // Assert
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void BaseEntity_AddDomainEvent_ShouldAllowMultipleEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent1 = new TestDomainEvent();
        var domainEvent2 = new TestDomainEvent();
        var domainEvent3 = new TestDomainEvent();
        
        // Act
        entity.AddTestDomainEvent(domainEvent1);
        entity.AddTestDomainEvent(domainEvent2);
        entity.AddTestDomainEvent(domainEvent3);
        
        // Assert
        entity.DomainEvents.Should().HaveCount(3);
        entity.DomainEvents.Should().Contain(domainEvent1);
        entity.DomainEvents.Should().Contain(domainEvent2);
        entity.DomainEvents.Should().Contain(domainEvent3);
    }

    [Fact]
    public void BaseEntity_RemoveDomainEvent_ShouldNotThrowIfEventNotExists()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        
        // Act & Assert
        var action = () => entity.RemoveDomainEvent(domainEvent);
        action.Should().NotThrow();
        entity.DomainEvents.Should().BeEmpty();
    }
} 