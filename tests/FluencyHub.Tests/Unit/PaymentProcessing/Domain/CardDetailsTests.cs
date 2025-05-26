using Xunit;
using FluentAssertions;
using FluencyHub.PaymentProcessing.Domain;

namespace FluencyHub.Tests.Unit.PaymentProcessing.Domain;

public class CardDetailsTests
{
    [Fact]
    public void CardDetails_Constructor_WithValidData_ShouldCreateValidCardDetails()
    {
        // Arrange
        var cardholderName = "João Silva";
        var cardNumber = "4532015112830366"; // Número válido para teste
        var expiryMonth = "12";
        var expiryYear = "2025";

        // Act
        var cardDetails = new CardDetails(cardholderName, cardNumber, expiryMonth, expiryYear);

        // Assert
        cardDetails.CardHolderName.Should().Be(cardholderName);
        cardDetails.MaskedCardNumber.Should().NotBeNullOrEmpty();
        cardDetails.ExpiryMonth.Should().Be(expiryMonth);
        cardDetails.ExpiryYear.Should().Be(expiryYear);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CardDetails_Constructor_WithInvalidCardholderName_ShouldThrowArgumentException(string? cardholderName)
    {
        // Arrange
        var cardNumber = "4532015112830366";
        var expiryMonth = "12";
        var expiryYear = "2025";

        // Act & Assert
        var action = () => new CardDetails(cardholderName!, cardNumber, expiryMonth, expiryYear);
        action.Should().Throw<ArgumentException>()
            .WithMessage("O nome do titular do cartão não pode estar vazio*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("12345678901234567890")]
    public void CardDetails_Constructor_WithInvalidCardNumber_ShouldThrowArgumentException(string? cardNumber)
    {
        // Arrange
        var cardholderName = "João Silva";
        var expiryMonth = "12";
        var expiryYear = "2025";

        // Act & Assert
        var action = () => new CardDetails(cardholderName, cardNumber!, expiryMonth, expiryYear);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("13")]
    [InlineData("abc")]
    public void CardDetails_Constructor_WithInvalidExpiryMonth_ShouldThrowArgumentException(string? expiryMonth)
    {
        // Arrange
        var cardholderName = "João Silva";
        var cardNumber = "4532015112830366";
        var expiryYear = "2025";

        // Act & Assert
        var action = () => new CardDetails(cardholderName, cardNumber, expiryMonth!, expiryYear);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("2020")]
    [InlineData("abc")]
    [InlineData("20")]
    public void CardDetails_Constructor_WithInvalidExpiryYear_ShouldThrowArgumentException(string? expiryYear)
    {
        // Arrange
        var cardholderName = "João Silva";
        var cardNumber = "4532015112830366";
        var expiryMonth = "12";

        // Act & Assert
        var action = () => new CardDetails(cardholderName, cardNumber, expiryMonth, expiryYear!);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CardDetails_Constructor_WithValidCardNumber_ShouldMaskCardNumber()
    {
        // Arrange
        var cardholderName = "João Silva";
        var cardNumber = "4532015112830366";
        var expiryMonth = "12";
        var expiryYear = "2025";

        // Act
        var cardDetails = new CardDetails(cardholderName, cardNumber, expiryMonth, expiryYear);

        // Assert
        cardDetails.MaskedCardNumber.Should().StartWith("453201");
        cardDetails.MaskedCardNumber.Should().EndWith("0366");
        cardDetails.MaskedCardNumber.Should().Contain("*");
    }

    [Fact]
    public void CardDetails_Constructor_WithExpiredDate_ShouldThrowArgumentException()
    {
        // Arrange
        var cardholderName = "João Silva";
        var cardNumber = "4532015112830366";
        var expiryMonth = "01";
        var expiryYear = "2020";

        // Act & Assert
        var action = () => new CardDetails(cardholderName, cardNumber, expiryMonth, expiryYear);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Data de validade inválida");
    }

    [Fact]
    public void CardDetails_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var cardDetails1 = new CardDetails("João Silva", "4532015112830366", "12", "2025");
        var cardDetails2 = new CardDetails("João Silva", "4532015112830366", "12", "2025");

        // Act & Assert
        cardDetails1.Should().Be(cardDetails2);
        cardDetails1.GetHashCode().Should().Be(cardDetails2.GetHashCode());
    }

    [Fact]
    public void CardDetails_Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var cardDetails1 = new CardDetails("João Silva", "4532015112830366", "12", "2025");
        var cardDetails2 = new CardDetails("Maria Silva", "4532015112830366", "12", "2025");

        // Act & Assert
        cardDetails1.Should().NotBe(cardDetails2);
    }

    [Fact]
    public void GetMaskedCardNumber_ShouldReturnMaskedNumber()
    {
        // Arrange
        var cardDetails = new CardDetails("João Silva", "4532015112830366", "12", "2025");

        // Act
        var maskedNumber = cardDetails.GetMaskedCardNumber();

        // Assert
        maskedNumber.Should().Be("****-****-****-0366");
    }

    [Fact]
    public void IsExpired_WithExpiredCard_ShouldReturnTrue()
    {
        // Arrange - Criar um cartão válido primeiro
        var cardDetails = new CardDetails("João Silva", "4532015112830366", "12", "2025");
        
        // Usar reflection para alterar o ano para um valor expirado
        var expiryYearProperty = typeof(CardDetails).GetProperty("ExpiryYear");
        expiryYearProperty?.SetValue(cardDetails, "23");

        // Act
        var isExpired = cardDetails.IsExpired();

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithValidCard_ShouldReturnFalse()
    {
        // Arrange
        var futureYear = (DateTime.Now.Year + 2).ToString();
        var cardDetails = new CardDetails("João Silva", "4532015112830366", "12", futureYear);

        // Act
        var isExpired = cardDetails.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }
} 