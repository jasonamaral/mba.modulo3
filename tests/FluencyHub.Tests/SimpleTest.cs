using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests;

public class SimpleTest
{
    [Fact]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var value = 42;

        // Act
        var result = value * 2;

        // Assert
        result.Should().Be(84);
    }
} 