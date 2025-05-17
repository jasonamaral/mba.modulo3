using FluencyHub.Domain.Common;
using System;
using Xunit;

namespace FluencyHub.Tests.Domain.Common
{
    public class DomainEventTests
    {
        private class TestDomainEvent : DomainEvent
        {
            // Classe concreta para testar a classe abstrata DomainEvent
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var domainEvent = new TestDomainEvent();

            // Assert
            Assert.NotEqual(Guid.Empty, domainEvent.Id);
            Assert.True(DateTime.UtcNow.Subtract(domainEvent.OccurredOn).TotalSeconds < 1);
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            // Arrange & Act
            var domainEvent1 = new TestDomainEvent();
            var domainEvent2 = new TestDomainEvent();

            // Assert
            Assert.NotEqual(domainEvent1.Id, domainEvent2.Id);
        }
    }
} 