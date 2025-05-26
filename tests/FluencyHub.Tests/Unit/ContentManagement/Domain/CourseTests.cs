using FluentAssertions;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.SharedKernel.Enums;
using Xunit;

namespace FluencyHub.Tests.Unit.ContentManagement.Domain;

public class CourseTests
{
    private CourseContent CreateValidCourseContent()
    {
        return new CourseContent(
            "Syllabus completo",
            "Objetivos de aprendizado",
            "Pré-requisitos",
            "Público alvo",
            "Português",
            "Iniciante"
        );
    }

    private Course CreateValidCourse(string name = "Curso de Inglês", string description = "Curso completo de inglês", decimal price = 299.99m)
    {
        var content = CreateValidCourseContent();
        return new Course(name, description, content, price)
        {
            Name = name,
            Description = description,
            Content = content
        };
    }

    [Fact]
    public void Course_Constructor_ShouldCreateValidCourse()
    {
        // Arrange
        var name = "Curso de Inglês";
        var description = "Curso completo de inglês";
        var content = CreateValidCourseContent();
        var price = 299.99m;

        // Act
        var course = new Course(name, description, content, price)
        {
            Name = name,
            Description = description,
            Content = content
        };

        // Assert
        course.Name.Should().Be(name);
        course.Description.Should().Be(description);
        course.Content.Should().Be(content);
        course.Price.Should().Be(price);
        course.IsActive.Should().BeTrue();
        course.Status.Should().Be(CourseStatus.Draft);
        course.EnrollmentCount.Should().Be(0);
        course.Lessons.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Course_Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var description = "Descrição válida";
        var content = CreateValidCourseContent();
        var price = 299.99m;

        // Act & Assert
        var action = () => new Course(invalidName!, description, content, price)
        {
            Name = invalidName ?? string.Empty,
            Description = description,
            Content = content
        };
        action.Should().Throw<ArgumentException>()
            .WithMessage("Course name cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Course_Constructor_WithInvalidDescription_ShouldThrowArgumentException(string? invalidDescription)
    {
        // Arrange
        var name = "Nome válido";
        var content = CreateValidCourseContent();
        var price = 299.99m;

        // Act & Assert
        var action = () => new Course(name, invalidDescription!, content, price)
        {
            Name = name,
            Description = invalidDescription ?? string.Empty,
            Content = content
        };
        action.Should().Throw<ArgumentException>()
            .WithMessage("Description cannot be empty*");
    }

    [Fact]
    public void Course_Constructor_WithNullContent_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "Nome válido";
        var description = "Descrição válida";
        CourseContent content = null!;
        var price = 299.99m;

        // Act & Assert
        var action = () => new Course(name, description, content, price)
        {
            Name = name,
            Description = description,
            Content = content
        };
        action.Should().Throw<ArgumentException>()
            .WithMessage("Course content cannot be null*");
    }

    [Fact]
    public void Course_Constructor_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "Nome válido";
        var description = "Descrição válida";
        var content = CreateValidCourseContent();
        var price = -10m;

        // Act & Assert
        var action = () => new Course(name, description, content, price)
        {
            Name = name,
            Description = description,
            Content = content
        };
        action.Should().Throw<ArgumentException>()
            .WithMessage("Price cannot be negative*");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateCourse()
    {
        // Arrange
        var course = CreateValidCourse("Nome inicial", "Descrição inicial", 100m);
        var newName = "Nome atualizado";
        var newDescription = "Descrição atualizada";
        var newContent = CreateValidCourseContent();
        var newPrice = 200m;

        // Act
        course.UpdateDetails(newName, newDescription, newContent, newPrice);

        // Assert
        course.Name.Should().Be(newName);
        course.Description.Should().Be(newDescription);
        course.Content.Should().Be(newContent);
        course.Price.Should().Be(newPrice);
        course.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddLesson_WithValidData_ShouldAddLessonToCourse()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);
        var title = "Lição 1";
        var content = "Conteúdo da lição";
        var description = "Descrição da lição";
        var order = 1;
        var duration = 30;

        // Act
        var lesson = course.AddLesson(title, content, description, order, duration);

        // Assert
        lesson.Should().NotBeNull();
        lesson.Title.Should().Be(title);
        lesson.Content.Should().Be(content);
        lesson.Description.Should().Be(description);
        lesson.Order.Should().Be(order);
        lesson.DurationMinutes.Should().Be(duration);
        course.Lessons.Should().HaveCount(1);
        course.Lessons.Should().Contain(lesson);
    }

    [Fact]
    public void AddLesson_WhenCourseIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);
        course.Deactivate();

        // Act & Assert
        var action = () => course.AddLesson("Título", "Conteúdo", "Descrição", 1);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível adicionar lições a um curso inativo");
    }

    [Fact]
    public void PublishCourse_ShouldChangeStatusToPublished()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);

        // Act
        course.PublishCourse();

        // Assert
        course.Status.Should().Be(CourseStatus.Published);
        course.PublishedAt.Should().NotBeNull();
        course.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ArchiveCourse_ShouldChangeStatusToArchivedAndDeactivate()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);

        // Act
        course.ArchiveCourse();

        // Assert
        course.Status.Should().Be(CourseStatus.Archived);
        course.IsActive.Should().BeFalse();
        course.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);

        // Act
        course.Deactivate();

        // Assert
        course.IsActive.Should().BeFalse();
        course.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenDeactivated_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);
        course.Deactivate();

        // Act
        course.Activate();

        // Assert
        course.IsActive.Should().BeTrue();
        course.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void IncrementEnrollmentCount_ShouldIncreaseCount()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);
        var initialCount = course.EnrollmentCount;

        // Act
        course.IncrementEnrollmentCount();

        // Assert
        course.EnrollmentCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void RemoveLesson_WithValidLessonId_ShouldRemoveLessonAndReorderOthers()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);
        var lesson1 = course.AddLesson("Lição 1", "Conteúdo 1", "Descrição 1", 1);
        var lesson2 = course.AddLesson("Lição 2", "Conteúdo 2", "Descrição 2", 2);
        var lesson3 = course.AddLesson("Lição 3", "Conteúdo 3", "Descrição 3", 3);

        // Act
        course.RemoveLesson(lesson2.Id);

        // Assert
        course.Lessons.Should().HaveCount(2);
        course.Lessons.Should().NotContain(lesson2);
        course.Lessons.Should().Contain(lesson1);
        course.Lessons.Should().Contain(lesson3);
    }

    [Fact]
    public void RemoveLesson_WithInvalidLessonId_ShouldThrowArgumentException()
    {
        // Arrange
        var course = CreateValidCourse("Curso", "Descrição", 100m);
        var invalidId = Guid.NewGuid();

        // Act & Assert
        var action = () => course.RemoveLesson(invalidId);
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Lesson with ID {invalidId} not found*");
    }
} 