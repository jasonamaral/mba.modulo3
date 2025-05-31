using FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
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
using FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourseId;
using FluencyHub.ContentManagement.Application.Common.Models;
using SharedCourseDto = FluencyHub.SharedKernel.Queries.CourseDto;
using FluencyHub.SharedKernel.Common.Exceptions;

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
        
        // Adicionar algumas lições completadas
        courseProgress.AddCompletedLesson(Guid.NewGuid());
        courseProgress.AddCompletedLesson(Guid.NewGuid());

        var courseDto = new SharedCourseDto
        {
            Id = courseId,
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Price = 299.99m
        };

        var lessons = new List<LessonDto>
        {
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 1", Order = 1 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 2", Order = 2 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 3", Order = 3 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 4", Order = 4 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 5", Order = 5 }
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

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetLessonsByCourseIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        _mockLearningRepository
            .Setup(x => x.GetCourseProgressesByStudentIdAsync(studentId))
            .ReturnsAsync(new List<CourseProgress> { courseProgress });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(studentId);
        result.StudentName.Should().Be(student.FullName);
        result.TotalCourses.Should().Be(1);
        result.CompletedCourses.Should().Be(0);
        result.TotalLessonsAcrossAllCourses.Should().Be(5);
        result.CompletedLessonsAcrossAllCourses.Should().Be(courseProgress.GetCompletedLessonsCount());
        result.OverallProgressPercentage.Should().BeGreaterOrEqualTo(0);
        result.CourseProgresses.Should().HaveCount(1);
        
        var courseProgressDto = result.CourseProgresses.First();
        courseProgressDto.CourseId.Should().Be(courseId);
        courseProgressDto.CourseName.Should().Be(courseDto.Name);
        courseProgressDto.TotalLessons.Should().Be(5);
        courseProgressDto.CompletedLessons.Should().Be(courseProgress.GetCompletedLessonsCount());
        courseProgressDto.EnrollmentDate.Should().Be(enrollment.EnrollmentDate);
        courseProgressDto.LastActivityDate.Should().Be(courseProgress.LastUpdated);
        courseProgressDto.IsCompleted.Should().Be(courseProgress.IsCompleted);
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
            .WithMessage("Estudante com ID * não encontrado");
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
        result.TotalCourses.Should().Be(0);
        result.CompletedCourses.Should().Be(0);
        result.TotalLessonsAcrossAllCourses.Should().Be(0);
        result.CompletedLessonsAcrossAllCourses.Should().Be(0);
        result.OverallProgressPercentage.Should().Be(0);
        result.CourseProgresses.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCourseNotFound_ShouldContinueWithOtherCourses()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();
        var query = new GetStudentProgressQuery { StudentId = studentId };

        var student = CreateValidStudent(studentId);
        var enrollment1 = CreateValidEnrollment(studentId, courseId1);
        var enrollment2 = CreateValidEnrollment(studentId, courseId2);

        var courseDto2 = new SharedCourseDto
        {
            Id = courseId2,
            Name = "Curso de Espanhol",
            Description = "Curso completo de espanhol",
            Price = 199.99m
        };

        var lessons = new List<LessonDto>
        {
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 1", Order = 1 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 2", Order = 2 }
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockEnrollmentRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Enrollment> { enrollment1, enrollment2 });

        _mockMediator
            .Setup(x => x.Send(It.Is<GetCourseById>(q => q.CourseId == courseId1), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SharedCourseDto?)null);

        _mockMediator
            .Setup(x => x.Send(It.Is<GetCourseById>(q => q.CourseId == courseId2), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto2);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetLessonsByCourseIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        _mockLearningRepository
            .Setup(x => x.GetCourseProgressesByStudentIdAsync(studentId))
            .ReturnsAsync(new List<CourseProgress>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CourseProgresses.Should().HaveCount(1);
        result.CourseProgresses.First().CourseId.Should().Be(courseId2);
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
        var courseDto = new SharedCourseDto
        {
            Id = courseId,
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Price = 299.99m
        };

        var lessons = new List<LessonDto>
        {
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 1", Order = 1 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 2", Order = 2 }
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

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetLessonsByCourseIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        _mockLearningRepository
            .Setup(x => x.GetCourseProgressesByStudentIdAsync(studentId))
            .ReturnsAsync(new List<CourseProgress>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CompletedLessonsAcrossAllCourses.Should().Be(0);
        result.OverallProgressPercentage.Should().Be(0);
        result.CourseProgresses.Should().HaveCount(1);
        result.CourseProgresses.First().IsCompleted.Should().BeFalse();
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
        var lessonId1 = Guid.NewGuid();
        var lessonId2 = Guid.NewGuid();
        var lessonId3 = Guid.NewGuid();
        courseProgress.AddCompletedLesson(lessonId1);
        courseProgress.AddCompletedLesson(lessonId2);
        courseProgress.AddCompletedLesson(lessonId3);

        var courseDto = new SharedCourseDto
        {
            Id = courseId,
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Price = 299.99m
        };

        var lessons = new List<LessonDto>
        {
            new LessonDto { Id = lessonId1, Title = "Lição 1", Order = 1 },
            new LessonDto { Id = lessonId2, Title = "Lição 2", Order = 2 },
            new LessonDto { Id = lessonId3, Title = "Lição 3", Order = 3 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 4", Order = 4 },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lição 5", Order = 5 }
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

        _mockMediator
            .Setup(x => x.Send(It.IsAny<GetLessonsByCourseIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        _mockLearningRepository
            .Setup(x => x.GetCourseProgressesByStudentIdAsync(studentId))
            .ReturnsAsync(new List<CourseProgress> { courseProgress });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CompletedLessonsAcrossAllCourses.Should().Be(3);
        result.OverallProgressPercentage.Should().Be(60); // 3/5 * 100 = 60%
        
        var courseProgressDto = result.CourseProgresses.First();
        courseProgressDto.LessonProgress.Should().HaveCount(5);
        courseProgressDto.LessonProgress.Count(lp => lp.IsCompleted).Should().Be(3);
    }

    private Student CreateValidStudent(Guid studentId)
    {
        var student = new Student(
            firstName: "João",
            lastName: "Silva",
            email: "joao.silva@email.com",
            dateOfBirth: new DateTime(1990, 5, 15));

        // Usar reflexão para definir o ID
        typeof(Student).GetProperty("Id")?.SetValue(student, studentId);

        return student;
    }

    private Enrollment CreateValidEnrollment(Guid studentId, Guid courseId)
    {
        var courseReference = new TestCourseReference(courseId, "Test Course", "Test Description", 100m);
        var enrollment = new Enrollment(studentId, courseId, 100m)
        {
            Student = CreateValidStudent(studentId),
            Course = courseReference
        };
        
        // Usar reflexão para definir o ID se necessário
        typeof(Enrollment).GetProperty("Id")?.SetValue(enrollment, Guid.NewGuid());
        
        return enrollment;
    }

    private CourseProgress CreateValidCourseProgress(Guid courseId)
    {
        return new CourseProgress(courseId);
    }
}

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