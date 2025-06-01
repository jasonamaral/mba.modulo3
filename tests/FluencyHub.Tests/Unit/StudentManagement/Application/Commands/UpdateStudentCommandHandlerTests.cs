using FluencyHub.StudentManagement.Application.Commands.UpdateStudent;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using Moq;
using Xunit;
using FluentAssertions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Commands;

public class UpdateStudentCommandHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly UpdateStudentCommandHandler _handler;

    public UpdateStudentCommandHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _handler = new UpdateStudentCommandHandler(_mockStudentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStudent_WhenValidCommand()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var existingStudent = new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));
        
        var command = new UpdateStudentCommand
        {
            Id = studentId,
            FirstName = "João Updated",
            LastName = "Silva Updated",
            Email = "joao.updated@email.com",
            PhoneNumber = "+5511999999999",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Nova Rua",
            City = "São Paulo",
            State = "SP",
            Country = "Brasil"
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(existingStudent);

        _mockStudentRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
        _mockStudentRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        existingStudent.FirstName.Should().Be(command.FirstName);
        existingStudent.LastName.Should().Be(command.LastName);
        existingStudent.Email.Should().Be(command.Email);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStudentNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var command = new UpdateStudentCommand
        {
            Id = studentId,
            FirstName = "João",
            LastName = "Silva",
            Email = "joao@email.com",
            PhoneNumber = "+5511999999999",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Estudante com ID {command.Id} não encontrado");
        
        _mockStudentRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHandleRepositoryException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var existingStudent = new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));
        
        var command = new UpdateStudentCommand
        {
            Id = studentId,
            FirstName = "João Updated",
            LastName = "Silva Updated",
            Email = "joao.updated@email.com",
            PhoneNumber = "+5511999999999",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(existingStudent);

        _mockStudentRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Erro no banco de dados"));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro no banco de dados");
    }
} 