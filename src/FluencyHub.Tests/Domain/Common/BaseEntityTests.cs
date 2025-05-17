using FluencyHub.Domain.Common;
using Xunit;

namespace FluencyHub.Tests.Domain.Common
{
    public class BaseEntityTests
    {
        private class TestEntity : BaseEntity
        {
            // Classe concreta para testar a classe abstrata BaseEntity
        }

        private class TestDomainEvent : DomainEvent
        {
            // Classe concreta para testar DomainEvent
        }

        [Fact]
        public void AddDomainEvent_ShouldAddEventToCollection()
        {
            // Arrange
            var entity = new TestEntity();
            var domainEvent = new TestDomainEvent();

            // Act
            entity.AddDomainEvent(domainEvent);

            // Assert
            Assert.Single(entity.DomainEvents);
            Assert.Contains(domainEvent, entity.DomainEvents);
        }

        [Fact]
        public void RemoveDomainEvent_ShouldRemoveEventFromCollection()
        {
            // Arrange
            var entity = new TestEntity();
            var domainEvent = new TestDomainEvent();
            entity.AddDomainEvent(domainEvent);

            // Act
            entity.RemoveDomainEvent(domainEvent);

            // Assert
            Assert.Empty(entity.DomainEvents);
        }

        [Fact]
        public void ClearDomainEvents_ShouldRemoveAllEventsFromCollection()
        {
            // Arrange
            var entity = new TestEntity();
            entity.AddDomainEvent(new TestDomainEvent());
            entity.AddDomainEvent(new TestDomainEvent());
            entity.AddDomainEvent(new TestDomainEvent());

            // Act
            entity.ClearDomainEvents();

            // Assert
            Assert.Empty(entity.DomainEvents);
        }

        [Fact]
        public void NewEntity_ShouldHaveEmptyDomainEvents()
        {
            // Arrange & Act
            var entity = new TestEntity();

            // Assert
            Assert.Empty(entity.DomainEvents);
        }
    }
} 