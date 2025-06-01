using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluentValidation.TestHelper;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Validators;

public class CreateStudentCommandValidatorTests
{
    private readonly CreateStudentCommandValidator _validator;

    public CreateStudentCommandValidatorTests()
    {
        _validator = new CreateStudentCommandValidator();
    }

    [Fact]
    public void ShouldValidate_WhenValidCommand()
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@email.com",
            Password = "Password123!",
            PhoneNumber = "+5511999999999",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Rua das Flores, 123",
            City = "São Paulo",
            State = "SP",
            Country = "Brasil",
            PostalCode = "01234-567"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ShouldFail_WhenFirstNameEmpty(string firstName)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { FirstName = firstName };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("O nome é obrigatório");
    }

    [Fact]
    public void ShouldFail_WhenFirstNameIsNull()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { FirstName = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("O nome é obrigatório");
    }

    [Fact]
    public void ShouldFail_WhenFirstNameTooShort()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { FirstName = "A" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("O nome deve ter pelo menos 2 caracteres");
    }

    [Fact]
    public void ShouldFail_WhenFirstNameTooLong()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { FirstName = new string('A', 101) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("O nome não deve exceder 100 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ShouldFail_WhenLastNameEmpty(string lastName)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { LastName = lastName };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("O sobrenome é obrigatório");
    }

    [Fact]
    public void ShouldFail_WhenLastNameIsNull()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { LastName = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("O sobrenome é obrigatório");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@email.com")]
    [InlineData("email@")]
    [InlineData("")]
    public void ShouldFail_WhenInvalidEmail(string email)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Email = email };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("password")]
    [InlineData("PASSWORD")]
    [InlineData("Password")]
    [InlineData("Password123")]
    public void ShouldFail_WhenInvalidPassword(string password)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Password = password };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("0999999999")]
    [InlineData("invalid-phone")]
    [InlineData("")]
    public void ShouldFail_WhenInvalidPhoneNumber(string phoneNumber)
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { PhoneNumber = phoneNumber };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void ShouldFail_WhenDateOfBirthTooYoung()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { DateOfBirth = DateTime.Today.AddYears(-10) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("O estudante deve ter pelo menos 13 anos e não mais de 120 anos");
    }

    [Fact]
    public void ShouldFail_WhenDateOfBirthTooOld()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { DateOfBirth = DateTime.Today.AddYears(-130) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage("O estudante deve ter pelo menos 13 anos e não mais de 120 anos");
    }

    [Fact]
    public void ShouldFail_WhenAddressTooLong()
    {
        // Arrange
        var command = CreateValidCommand();
        command = command with { Address = new string('A', 501) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage("O endereço não deve exceder 500 caracteres");
    }

    private static CreateStudentCommand CreateValidCommand()
    {
        return new CreateStudentCommand
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@email.com",
            Password = "Password123!",
            PhoneNumber = "+5511999999999",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Rua das Flores, 123",
            City = "São Paulo",
            State = "SP",
            Country = "Brasil",
            PostalCode = "01234-567"
        };
    }
} 