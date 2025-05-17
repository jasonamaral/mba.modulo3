using System;
using FluencyHub.PaymentProcessing.Domain;
using Xunit;

namespace FluencyHub.Tests.Domain.PaymentProcessing
{
    public class CardDetailsTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateCardDetails()
        {
            // Arrange
            var cardHolderName = "John Doe";
            var cardNumber = "4111111111111111"; // Valid test Visa card number
            var expiryMonth = "12";
            var expiryYear = (DateTime.UtcNow.Year + 1).ToString();

            // Act
            var cardDetails = new CardDetails(cardHolderName, cardNumber, expiryMonth, expiryYear);

            // Assert
            Assert.Equal(cardHolderName, cardDetails.CardHolderName);
            Assert.Equal("411111******1111", cardDetails.MaskedCardNumber);
            Assert.Equal(expiryMonth, cardDetails.ExpiryMonth);
            Assert.Equal(expiryYear, cardDetails.ExpiryYear);
        }

        [Theory]
        [InlineData("", "4111111111111111", "12", "2030")]
        [InlineData("John Doe", "", "12", "2030")]
        [InlineData("John Doe", "4111111111111111", "", "2030")]
        [InlineData("John Doe", "4111111111111111", "12", "")]
        public void Constructor_WithEmptyParameters_ShouldThrowArgumentException(
            string cardHolderName,
            string cardNumber,
            string expiryMonth,
            string expiryYear)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CardDetails(
                cardHolderName,
                cardNumber,
                expiryMonth,
                expiryYear));
        }

        [Theory]
        [InlineData("1234567890123")] // Too short
        [InlineData("12345678901234567890")] // Too long
        [InlineData("1234567890123456")] // Invalid by Luhn algorithm
        public void Constructor_WithInvalidCardNumber_ShouldThrowArgumentException(string cardNumber)
        {
            // Arrange
            var cardHolderName = "John Doe";
            var expiryMonth = "12";
            var expiryYear = (DateTime.UtcNow.Year + 1).ToString();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CardDetails(
                cardHolderName,
                cardNumber,
                expiryMonth,
                expiryYear));
        }

        [Theory]
        [InlineData("00", "2030")] // Invalid month
        [InlineData("13", "2030")] // Invalid month
        [InlineData("12", "2000")] // Past year
        public void Constructor_WithInvalidExpiryDate_ShouldThrowArgumentException(
            string expiryMonth,
            string expiryYear)
        {
            // Arrange
            var cardHolderName = "John Doe";
            var cardNumber = "4111111111111111"; // Valid test Visa card number

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CardDetails(
                cardHolderName,
                cardNumber,
                expiryMonth,
                expiryYear));
        }

        [Fact]
        public void Constructor_WithFormattedCardNumber_ShouldCreateCardDetails()
        {
            // Arrange
            var cardHolderName = "John Doe";
            var cardNumber = "4111-1111-1111-1111"; // Valid test Visa card number with dashes
            var expiryMonth = "12";
            var expiryYear = (DateTime.UtcNow.Year + 1).ToString();

            // Act
            var cardDetails = new CardDetails(cardHolderName, cardNumber, expiryMonth, expiryYear);

            // Assert
            Assert.Equal("411111******1111", cardDetails.MaskedCardNumber);
        }

        [Fact]
        public void Constructor_WithTwoDigitYear_ShouldCreateCardDetails()
        {
            // Arrange
            var cardHolderName = "John Doe";
            var cardNumber = "4111111111111111";
            var expiryMonth = "12";
            var expiryYear = (DateTime.UtcNow.Year + 1 - 2000).ToString(); // Two-digit year

            // Act
            var cardDetails = new CardDetails(cardHolderName, cardNumber, expiryMonth, expiryYear);

            // Assert
            Assert.Equal(expiryYear, cardDetails.ExpiryYear);
        }
    }
} 