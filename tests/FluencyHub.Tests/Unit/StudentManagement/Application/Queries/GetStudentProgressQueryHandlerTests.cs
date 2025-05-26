using FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;
using FluencyHub.StudentManagement.Application.Common.Exceptions;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Queries;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;
using IEnrollmentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IEnrollmentRepository;
using ILearningRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.ILearningRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetStudentProgressQueryHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly Mock<ILearningRepository> _mockLearningRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<GetStudentProgressQueryHandler>> _mockLogger;
    private readonly GetStudentProgressQueryHandler _handler;

    public GetStudentProgressQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _mockLearningRepository = new Mock<ILearningRepository>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<GetStudentProgressQueryHandler>>();
        
        _handler = new GetStudentProgressQueryHandler(
            _mockStudentRepository.Object,
            _mockEnrollmentRepository.Object,
            _mockLearningRepository.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidStudentAndEnrollments_ShouldReturnProgress()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetStudentProgressQuery { StudentId = studentId };

        var student = CreateValidStudent(studentId);
        var enrollment = CreateValidEnrollment(studentId, courseId);
        var courseProgress = CreateValidCourseProgress(courseId);
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

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Enrollment> { enrollment });

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mockLearningRepository
            .Setup(x => x.GetCourseProgressesByStudentIdAsync(studentId))
            .ReturnsAsync(new List<CourseProgress> { courseProgress });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(studentId);
        result.StudentName.Should().Be(student.FullName);
        result.CourseId.Should().Be(courseId);
        result.CourseName.Should().Be(courseDto.Name);
        result.TotalLessons.Should().Be(10); // Valor padrão
        result.CompletedLessons.Should().Be(courseProgress.GetCompletedLessonsCount());
        result.ProgressPercentage.Should().BeGreaterOrEqualTo(0);
        result.EnrollmentDate.Should().Be(enrollment.EnrollmentDate);
        result.LastActivityDate.Should().Be(courseProgress.LastUpdated);
        result.IsCompleted.Should().Be(courseProgress.IsCompleted);
    }

    [Fact]
    public async Task Handle_WhenStudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentProgressQuery { StudentId = studentId };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Student with ID {studentId} not found");
    }

    [Fact]
    public async Task Handle_WhenStudentHasNoEnrollments_ShouldReturnEmptyProgress()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentProgressQuery { StudentId = studentId };

        var student = CreateValidStudent(studentId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Enrollment>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(studentId);
        result.StudentName.Should().Be(student.FullName);
        result.CourseId.Should().Be(Guid.Empty);
        result.CourseName.Should().Be("Nenhum curso matriculado");
        result.TotalLessons.Should().Be(0);
        result.CompletedLessons.Should().Be(0);
        result.ProgressPercentage.Should().Be(0);
        result.IsCompleted.Should().BeFalse();
        result.LessonProgress.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCourseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetStudentProgressQuery { StudentId = studentId };

        var student = CreateValidStudent(studentId);
        var enrollment = CreateValidEnrollment(studentId, courseId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Enrollment> { enrollment });

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseDto?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Course with ID {courseId} not found");
    }

    [Fact]
    public async Task Handle_WhenNoCourseProgress_ShouldReturnZeroProgress()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetStudentProgressQuery { StudentId = studentId };

        var student = CreateValidStudent(studentId);
        var enrollment = CreateValidEnrollment(studentId, courseId);
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

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Enrollment> { enrollment });

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mockLearningRepository
            .Setup(x => x.GetCourseProgressesByStudentIdAsync(studentId))
            .ReturnsAsync(new List<CourseProgress>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CompletedLessons.Should().Be(0);
        result.ProgressPercentage.Should().Be(0);
        result.IsCompleted.Should().BeFalse();
        result.LastActivityDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCompletedLessons_ShouldCalculateCorrectProgress()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetStudentProgressQuery { StudentId = studentId };

        var student = CreateValidStudent(studentId);
        var enrollment = CreateValidEnrollment(studentId, courseId);
        var courseProgress = CreateValidCourseProgress(courseId);
        
        // Adicionar algumas lições completadas
        courseProgress.AddCompletedLesson(Guid.NewGuid());
        courseProgress.AddCompletedLesson(Guid.NewGuid());
        courseProgress.AddCompletedLesson(Guid.NewGuid());

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

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Enrollment> { enrollment });

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetCourseById>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);

        _mockLearningRepository
            .Setup(x => x.GetCourseProgressesByStudentIdAsync(studentId))
            .ReturnsAsync(new List<CourseProgress> { courseProgress });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CompletedLessons.Should().Be(3);
        result.ProgressPercentage.Should().Be(30); // 3/10 * 100 = 30%
        result.LessonProgress.Should().HaveCount(3);
        result.LessonProgress.Should().OnlyContain(lp => lp.IsCompleted);
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

    private Enrollment CreateValidEnrollment(Guid studentId, Guid courseId)
    {
        var enrollment = new Enrollment(studentId, courseId, 299.99m)
        {
            Student = CreateValidStudent(studentId),
            Course = new TestCourseReference(courseId, "Test Course", "Description", 299.99m)
        };
        
        enrollment.ActivateEnrollment();
        return enrollment;
    }

    private CourseProgress CreateValidCourseProgress(Guid courseId)
    {
        return new CourseProgress(courseId);
    }
}

// Classe auxiliar para testes (reutilizada do teste anterior)
public class TestCourseReference : FluencyHub.SharedKernel.Contracts.ICourse
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