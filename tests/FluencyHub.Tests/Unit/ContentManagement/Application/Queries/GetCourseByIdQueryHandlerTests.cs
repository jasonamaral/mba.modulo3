using FluencyHub.ContentManagement.Application.Queries.GetCourseById;
using FluencyHub.ContentManagement.Domain;
using Moq;
using Xunit;
using FluentAssertions;
using AppInterfaces = FluencyHub.ContentManagement.Application.Common.Interfaces;
using AppModels = FluencyHub.ContentManagement.Application.Common.Models;

namespace FluencyHub.Tests.Unit.ContentManagement.Application.Queries;

public class GetCourseByIdQueryHandlerTests
{
    private readonly Mock<AppInterfaces.ICourseRepository> _mockCourseRepository;
    private readonly GetCourseByIdQueryHandler _handler;

    public GetCourseByIdQueryHandlerTests()
    {
        _mockCourseRepository = new Mock<AppInterfaces.ICourseRepository>();
        _handler = new GetCourseByIdQueryHandler(_mockCourseRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCourse_WhenCourseExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course(
            "Curso de Inglês",
            "Curso completo de inglês",
            new CourseContent(
                "Gramática e vocabulário",
                "Comunicação fluente",
                "Ensino médio",
                "Estudantes",
                "Inglês",
                "Intermediário"),
            299.99m)
        {
            Name = "Curso de Inglês",
            Description = "Curso completo de inglês",
            Content = new CourseContent(
                "Gramática e vocabulário",
                "Comunicação fluente",
                "Ensino médio",
                "Estudantes",
                "Inglês",
                "Intermediário")
        };

        var query = new GetCourseByIdQuery { CourseId = courseId };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(course.Id);
        result.Name.Should().Be(course.Name);
        result.Description.Should().Be(course.Description);
        result.Syllabus.Should().Be(course.Content.Syllabus);
        result.LearningObjectives.Should().Be(course.Content.LearningObjectives);
        result.PreRequisites.Should().Be(course.Content.PreRequisites);
        result.TargetAudience.Should().Be(course.Content.TargetAudience);
        result.Language.Should().Be(course.Content.Language);
        result.Level.Should().Be(course.Content.Level);
        result.Price.Should().Be(course.Price);
        result.IsActive.Should().Be(course.IsActive);
        
        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var query = new GetCourseByIdQuery { CourseId = courseId };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(() => null!);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Curso com ID {courseId} não encontrado");
        
        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCourseWithCorrectMapping_WhenCourseExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course(
            "Curso de Espanhol",
            "Aprenda espanhol rapidamente",
            new CourseContent(
                "Verbos e conjugações",
                "Conversação básica",
                "Nenhum",
                "Iniciantes",
                "Espanhol",
                "Básico"),
            199.99m)
        {
            Name = "Curso de Espanhol",
            Description = "Aprenda espanhol rapidamente",
            Content = new CourseContent(
                "Verbos e conjugações",
                "Conversação básica",
                "Nenhum",
                "Iniciantes",
                "Espanhol",
                "Básico")
        };

        var query = new GetCourseByIdQuery { CourseId = courseId };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeOfType<AppModels.CourseDto>();
        result.CreatedAt.Should().Be(course.CreatedAt);
        result.UpdatedAt.Should().Be(course.UpdatedAt);
    }
} 