using FluencyHub.StudentManagement.Application.Commands.EnrollStudent;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
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

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Commands;

public class EnrollStudentCommandHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<EnrollStudentCommandHandler>> _mockLogger;
    private readonly EnrollStudentCommandHandler _handler;

    public EnrollStudentCommandHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<EnrollStudentCommandHandler>>();
        
        _handler = new EnrollStudentCommandHandler(
            _mockStudentRepository.Object,
            _mockEnrollmentRepository.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEnrollmentAndReturnId()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            DiscountPercentage = null
        };

        var student = CreateValidStudent(studentId);
        var courseDto = new CourseDto
        {
            Id = courseId,
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Price = 299.99m
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CourseExists>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync((Enrollment?)null);

        _mockEnrollmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Enrollment>()))
            .Returns(Task.CompletedTask);

        _mockEnrollmentRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockEnrollmentRepository.Verify(x => x.AddAsync(It.Is<Enrollment>(e => 
            e.StudentId == studentId &&
            e.CourseId == courseId &&
            e.Price == courseDto.Price)), Times.Once);
        _mockEnrollmentRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDiscount_ShouldApplyDiscountToPrice()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var originalPrice = 100m;
        var discountPercentage = 20m;
        var expectedPrice = 80m; // 100 - (100 * 0.20)

        var command = new EnrollStudentCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            DiscountPercentage = discountPercentage
        };

        var student = CreateValidStudent(studentId);
        var courseDto = new CourseDto
        {
            Id = courseId,
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Price = originalPrice
        };

        SetupValidScenario(studentId, courseId, student, courseDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockEnrollmentRepository.Verify(x => x.AddAsync(It.Is<Enrollment>(e => 
            e.Price == expectedPrice)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Student with ID {studentId} not found");
    }

    [Fact]
    public async Task Handle_WhenStudentIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow
        };

        var inactiveStudent = CreateValidStudent(studentId);
        inactiveStudent.Deactivate();

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(inactiveStudent);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot enroll an inactive student");
    }

    [Fact]
    public async Task Handle_WhenCourseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow
        };

        var student = CreateValidStudent(studentId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CourseExists>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Course with ID {courseId} not found");
    }

    [Fact]
    public async Task Handle_WhenStudentAlreadyEnrolledActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow
        };

        var student = CreateValidStudent(studentId);
        var courseDto = new CourseDto
        {
            Id = courseId,
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Price = 299.99m
        };

        var existingEnrollment = new Enrollment(studentId, courseId, 299.99m)
        {
            Student = student,
            Course = new TestCourseReference(courseId, "Test Course", "Description", 299.99m)
        };
        existingEnrollment.ActivateEnrollment();

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CourseExists>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync(existingEnrollment);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Student is already enrolled in course {courseId} with status {existingEnrollment.Status}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Handle_WithValidDiscountPercentages_ShouldCalculateCorrectPrice(decimal discountPercentage)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var originalPrice = 200m;
        var expectedPrice = originalPrice - (originalPrice * (discountPercentage / 100));

        var command = new EnrollStudentCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            DiscountPercentage = discountPercentage
        };

        var student = CreateValidStudent(studentId);
        var courseDto = new CourseDto
        {
            Id = courseId,
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Price = originalPrice
        };

        SetupValidScenario(studentId, courseId, student, courseDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockEnrollmentRepository.Verify(x => x.AddAsync(It.Is<Enrollment>(e => 
            e.Price == expectedPrice)), Times.Once);
    }

    private Student CreateValidStudent(Guid studentId)
    {
        var student = new Student(
            firstName: "João",
            lastName: "Silva",
            email: "joao.silva@email.com",
            dateOfBirth: new DateTime(1990, 5, 15))
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@email.com",
            LearningHistory = new LearningHistory(studentId)
        };

        // Use reflection to set the Id
        var idProperty = typeof(Student).BaseType?.GetProperty("Id");
        idProperty?.SetValue(student, studentId);

        return student;
    }

    private void SetupValidScenario(Guid studentId, Guid courseId, Student student, CourseDto courseDto)
    {
        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CourseExists>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync((Enrollment?)null);

        _mockEnrollmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Enrollment>()))
            .Returns(Task.CompletedTask);

        _mockEnrollmentRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}

// Classe auxiliar para testes
public class TestCourseReference : ICourse
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Language { get; } = "Portuguese";
    public string Level { get; } = "Beginner";
    public decimal Price { get; }
    public bool IsActive { get; } = true;

    public TestCourseReference(Guid id, string name, string description, decimal price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }
} 