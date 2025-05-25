using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using Moq;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Commands;

public class CreateStudentCommandHandlerTests
{
    private readonly Mock<FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository> _mockStudentRepository;
    private readonly CreateStudentCommandHandler _handler;

    public CreateStudentCommandHandlerTests()
    {
        _mockStudentRepository = new Mock<FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository>();
        _handler = new CreateStudentCommandHandler(_mockStudentRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateStudentAndReturnId()
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@email.com",
            DateOfBirth = new DateTime(1990, 5, 15),
            PhoneNumber = "+5511999999999"
        };

        var studentId = Guid.NewGuid();

        _mockStudentRepository
            .Setup(x => x.AddAsync(It.IsAny<Student>()))
            .Returns(Task.CompletedTask);

        _mockStudentRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockStudentRepository.Verify(x => x.AddAsync(It.Is<Student>(s => 
            s.FirstName == command.FirstName &&
            s.LastName == command.LastName &&
            s.Email == command.Email)), Times.Once);
        _mockStudentRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldThrowException()
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@email.com",
            DateOfBirth = new DateTime(1990, 5, 15),
            PhoneNumber = "+5511999999999"
        };

        _mockStudentRepository
            .Setup(x => x.AddAsync(It.IsAny<Student>()))
            .ThrowsAsync(new InvalidOperationException("Falha no repositório"));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Falha no repositório");
    }

    [Theory]
    [InlineData("", "Silva", "joao@email.com")]
    [InlineData("João", "", "joao@email.com")]
    [InlineData("João", "Silva", "")]
    public async Task Handle_WithInvalidData_ShouldThrowArgumentException(string firstName, string lastName, string email)
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DateOfBirth = new DateTime(1990, 5, 15),
            PhoneNumber = "+5511999999999"
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateStudentWithLearningHistory()
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = "Maria",
            LastName = "Santos",
            Email = "maria.santos@email.com",
            DateOfBirth = new DateTime(1985, 3, 20),
            PhoneNumber = "+5511888888888",
            Address = "Rua das Flores, 123",
            City = "São Paulo",
            State = "SP",
            Country = "Brasil"
        };

        _mockStudentRepository
            .Setup(x => x.AddAsync(It.IsAny<Student>()))
            .Returns(Task.CompletedTask);

        _mockStudentRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockStudentRepository.Verify(x => x.AddAsync(It.Is<Student>(s => 
            s.LearningHistory != null)), Times.Once);
    }
} 