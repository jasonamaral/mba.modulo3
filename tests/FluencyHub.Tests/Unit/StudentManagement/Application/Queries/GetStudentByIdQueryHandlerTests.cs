using Xunit;
using Moq;
using FluentAssertions;
using FluencyHub.StudentManagement.Application.Queries.GetStudentById;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetStudentByIdQueryHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly GetStudentByIdQueryHandler _handler;

    public GetStudentByIdQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _handler = new GetStudentByIdQueryHandler(_mockStudentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnStudent_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));

        var query = new GetStudentByIdQuery { StudentId = studentId };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(student.Id);
        result.FirstName.Should().Be(student.FirstName);
        result.LastName.Should().Be(student.LastName);
        result.Email.Should().Be(student.Email);
        result.IsActive.Should().Be(student.IsActive);
        result.FullName.Should().Be(student.FullName);
        
        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStudentNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentByIdQuery { StudentId = studentId };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Estudante com ID {studentId} não encontrado");
        
        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnStudentWithCounts_WhenStudentHasEnrollmentsAndCertificates()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student("Maria", "Santos", "maria@email.com", new DateTime(1985, 5, 15));

        var query = new GetStudentByIdQuery { StudentId = studentId };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EnrollmentsCount.Should().Be(student.Enrollments.Count);
        result.CertificatesCount.Should().Be(student.Certificates.Count);
    }
} 