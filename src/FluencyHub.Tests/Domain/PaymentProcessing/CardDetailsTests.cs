using FluencyHub.Domain.PaymentProcessing;
using System;
using Xunit;

namespace FluencyHub.Tests.Domain.PaymentProcessing;

public class CardDetailsTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        
        // Assert
        Assert.Equal("John Doe", cardDetails.CardHolderName);
        Assert.Contains("1111", cardDetails.MaskedCardNumber);
        Assert.Equal("12", cardDetails.ExpiryMonth);
        Assert.Equal("2030", cardDetails.ExpiryYear);
    }
    
    [Theory]
    [InlineData("", "4111111111111111", "12", "2030", "Card holder name cannot be empty")]
    [InlineData("John Doe", "", "12", "2030", "Card number cannot be empty")]
    [InlineData("John Doe", "4111111111111111", "", "2030", "Expiry month cannot be empty")]
    [InlineData("John Doe", "4111111111111111", "12", "", "Expiry year cannot be empty")]
    [InlineData("John Doe", "1234", "12", "2030", "Invalid card number")]
    public void Constructor_WithInvalidParameters_ThrowsArgumentException(
        string cardholderName, string cardNumber, string expiryMonth, string expiryYear, 
        string expectedErrorMessage)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new CardDetails(cardholderName, cardNumber, expiryMonth, expiryYear));
        Assert.Contains(expectedErrorMessage, exception.Message);
    }
    
    [Fact]
    public void Constructor_WithInvalidExpiryDate_ThrowsArgumentException()
    {
        // Arrange
        var pastYear = (DateTime.UtcNow.Year - 1).ToString();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new CardDetails("John Doe", "4111111111111111", "12", pastYear));
        Assert.Contains("Invalid expiry date", exception.Message);
    }
    
    [Fact]
    public void Constructor_WithInvalidExpiryMonth_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new CardDetails("John Doe", "4111111111111111", "13", "2030"));
        Assert.Contains("Invalid expiry date", exception.Message);
    }
    
    [Theory]
    [InlineData("4111111111111111")] // Valid Visa
    [InlineData("5555555555554444")] // Valid MasterCard
    [InlineData("378282246310005")] // Valid American Express
    public void Constructor_WithValidCardNumber_CreatesInstance(string cardNumber)
    {
        // Arrange & Act
        var futureYear = (DateTime.UtcNow.Year + 5).ToString();
        var cardDetails = new CardDetails("John Doe", cardNumber, "12", futureYear);
        
        // Assert
        Assert.NotNull(cardDetails);
        Assert.Equal("John Doe", cardDetails.CardHolderName);
    }
    
    [Fact]
    public void MaskedCardNumber_MasksMiddleDigits()
    {
        // Arrange
        var cardDetails = new CardDetails("John Doe", "4111111111111111", "12", "2030");
        
        // Act & Assert
        Assert.Contains("1111", cardDetails.MaskedCardNumber);
        Assert.DoesNotContain("41111111111111111", cardDetails.MaskedCardNumber);
    }
} 