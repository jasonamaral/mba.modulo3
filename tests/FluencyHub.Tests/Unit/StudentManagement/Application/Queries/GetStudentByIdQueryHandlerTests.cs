using Xunit;
using Moq;
using FluentAssertions;
using FluencyHub.StudentManagement.Application.Queries.GetStudentById;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using AppInterfaces = FluencyHub.StudentManagement.Application.Common.Interfaces;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetStudentByIdQueryHandlerTests
{
    private readonly Mock<AppInterfaces.IStudentRepository> _mockStudentRepository;
    private readonly GetStudentByIdQueryHandler _handler;

    public GetStudentByIdQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<AppInterfaces.IStudentRepository>();
        _handler = new GetStudentByIdQueryHandler(_mockStudentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnStudentDto_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = CreateValidStudent();
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
        result.FullName.Should().Be($"{student.FirstName} {student.LastName}");
        result.IsActive.Should().Be(student.IsActive);
        result.EnrollmentsCount.Should().Be(student.Enrollments.Count);
        result.CertificatesCount.Should().Be(student.Certificates.Count);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenStudentDoesNotExist()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentByIdQuery { StudentId = studentId };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));

        exception.Message.Should().Contain("Estudante com ID");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOnce_WhenHandlingQuery()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = CreateValidStudent();
        var query = new GetStudentByIdQuery { StudentId = studentId };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllProperties_WhenStudentHasCompleteInformation()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = CreateValidStudentWithCompleteInfo();
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
        result.PhoneNumber.Should().Be(student.PhoneNumber ?? string.Empty);
        result.DateOfBirth.Should().Be(student.DateOfBirth);
        result.Address.Should().Be(student.Address);
        result.City.Should().Be(student.City);
        result.State.Should().Be(student.State);
        result.Country.Should().Be(student.Country);
        result.PostalCode.Should().Be(student.PostalCode);
        result.IsActive.Should().Be(student.IsActive);
        result.CreatedAt.Should().Be(student.CreatedAt);
        result.UpdatedAt.Should().Be(student.UpdatedAt);
        result.FullName.Should().Be(student.FullName);
    }

    [Fact]
    public async Task Handle_ShouldHandleNullPhoneNumber_WhenStudentHasNoPhoneNumber()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = CreateValidStudent();
        var query = new GetStudentByIdQuery { StudentId = studentId };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PhoneNumber.Should().Be(string.Empty);
    }

    private static Student CreateValidStudent()
    {
        return new Student("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));
    }

    private static Student CreateValidStudentWithCompleteInfo()
    {
        var student = CreateValidStudent();
        
        student.Update(
            "John",
            "Doe", 
            "john.doe@example.com",
            "+1234567890",
            "123 Main St",
            "New York",
            "NY",
            "USA");

        return student;
    }
} 