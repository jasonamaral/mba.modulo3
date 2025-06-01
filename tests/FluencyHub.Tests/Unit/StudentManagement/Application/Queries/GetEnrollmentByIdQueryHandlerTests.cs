using FluencyHub.StudentManagement.Application.Queries.GetEnrollmentById;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Queries;
using FluencyHub.SharedKernel.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IEnrollmentRepository;
using FluencyHub.SharedKernel.Common.Exceptions;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetEnrollmentByIdQueryHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<GetEnrollmentByIdQueryHandler>> _mockLogger;
    private readonly GetEnrollmentByIdQueryHandler _handler;

    public GetEnrollmentByIdQueryHandlerTests()
    {
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<GetEnrollmentByIdQueryHandler>>();

        _handler = new GetEnrollmentByIdQueryHandler(
            _mockEnrollmentRepository.Object,
            _mockStudentRepository.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Enrollment CreateTestEnrollment(Guid studentId, Guid courseId, decimal price = 100m)
    {
        var mockCourse = new Mock<ICourse>();
        mockCourse.Setup(x => x.Id).Returns(courseId);
        mockCourse.Setup(x => x.Name).Returns("Test Course");
        mockCourse.Setup(x => x.Price).Returns(price);

        var student = new Student("John", "Doe", "john.doe@example.com", DateTime.Parse("1990-01-01"));

        return new Enrollment(studentId, courseId, price)
        {
            Student = student,
            Course = mockCourse.Object
        };
    }

    [Fact]
    public async Task Handle_WhenEnrollmentExists_ShouldReturnEnrollmentDto()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var enrollment = CreateTestEnrollment(studentId, courseId);
        var student = new Student("John", "Doe", "john.doe@example.com", DateTime.Parse("1990-01-01"));

        var courseDto = new FluencyHub.SharedKernel.Queries.CourseDto
        {
            Id = courseId,
            Name = "Test Course",
            Description = "Test Description",
            Price = 100m
        };

        _mockEnrollmentRepository.Setup(x => x.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);

        _mockStudentRepository.Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FluencyHub.SharedKernel.Queries.CourseDto?)courseDto);

        var query = new GetEnrollmentByIdQuery { EnrollmentId = enrollmentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(enrollment.Id);
        result.StudentId.Should().Be(studentId);
        result.CourseId.Should().Be(courseId);
        result.StudentName.Should().Be("John Doe");
        result.CourseName.Should().Be("Test Course");
        result.Price.Should().Be(100m);
        result.FinalPrice.Should().Be(100m);
        result.Status.Should().Be(enrollment.Status.ToString());
    }

    [Fact]
    public async Task Handle_WhenEnrollmentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();

        _mockEnrollmentRepository.Setup(x => x.GetByIdAsync(enrollmentId))
            .ReturnsAsync((Enrollment?)null);

        var query = new GetEnrollmentByIdQuery { EnrollmentId = enrollmentId };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var enrollment = CreateTestEnrollment(studentId, courseId);

        _mockEnrollmentRepository.Setup(x => x.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);

        _mockStudentRepository.Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        var query = new GetEnrollmentByIdQuery { EnrollmentId = enrollmentId };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenCourseInfoNotAvailable_ShouldUseDefaultCourseName()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var enrollment = CreateTestEnrollment(studentId, courseId);
        var student = new Student("John", "Doe", "john.doe@example.com", DateTime.Parse("1990-01-01"));

        _mockEnrollmentRepository.Setup(x => x.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);

        _mockStudentRepository.Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Course service unavailable"));

        var query = new GetEnrollmentByIdQuery { EnrollmentId = enrollmentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CourseName.Should().Be("Nome do Curso"); // Default value
    }

    [Fact]
    public async Task Handle_WhenEnrollmentIsCompleted_ShouldSetIsCompletedTrue()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var enrollment = CreateTestEnrollment(studentId, courseId);
        // Simular conclusão da matrícula
        enrollment.ActivateEnrollment();
        enrollment.CompleteEnrollment();

        var student = new Student("John", "Doe", "john.doe@example.com", DateTime.Parse("1990-01-01"));

        _mockEnrollmentRepository.Setup(x => x.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);

        _mockStudentRepository.Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        var query = new GetEnrollmentByIdQuery { EnrollmentId = enrollmentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsCompleted.Should().BeTrue();
        result.CompletionDate.Should().NotBeNull();
    }
} 