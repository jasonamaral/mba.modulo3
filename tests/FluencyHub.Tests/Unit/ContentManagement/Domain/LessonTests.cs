using FluentAssertions;
using FluencyHub.ContentManagement.Domain;
using Xunit;

namespace FluencyHub.Tests.Unit.ContentManagement.Domain;

public class LessonTests
{
    private Course CreateValidCourse()
    {
        var content = new CourseContent(
            "Syllabus completo",
            "Objetivos de aprendizado",
            "Pré-requisitos",
            "Público alvo",
            "Português",
            "Iniciante"
        );
        var name = "Curso de Teste";
        var description = "Descrição do curso";
        return new Course(name, description, content, 100m)
        {
            Name = name,
            Description = description,
            Content = content
        };
    }

    [Fact]
    public void Lesson_Constructor_ShouldCreateValidLesson()
    {
        // Arrange
        var course = CreateValidCourse();
        var title = "Lição 1";
        var content = "Conteúdo da lição";
        var description = "Descrição da lição";
        var order = 1;
        var duration = 30;

        // Act
        var lesson = new Lesson(title, content, description, course, order, duration);

        // Assert
        lesson.Title.Should().Be(title);
        lesson.Content.Should().Be(content);
        lesson.Description.Should().Be(description);
        lesson.Course.Should().Be(course);
        lesson.CourseId.Should().Be(course.Id);
        lesson.Order.Should().Be(order);
        lesson.DurationMinutes.Should().Be(duration);
        lesson.CompletionCount.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Lesson_Constructor_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange
        var course = CreateValidCourse();
        var content = "Conteúdo válido";
        var description = "Descrição válida";

        // Act & Assert
        var action = () => new Lesson(invalidTitle!, content, description, course, 1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("O título não pode estar vazio*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Lesson_Constructor_WithInvalidContent_ShouldThrowArgumentException(string? invalidContent)
    {
        // Arrange
        var course = CreateValidCourse();
        var title = "Título válido";
        var description = "Descrição válida";

        // Act & Assert
        var action = () => new Lesson(title, invalidContent!, description, course, 1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("O conteúdo não pode estar vazio*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Lesson_Constructor_WithInvalidDescription_ShouldThrowArgumentException(string? invalidDescription)
    {
        // Arrange
        var course = CreateValidCourse();
        var title = "Título válido";
        var content = "Conteúdo válido";

        // Act & Assert
        var action = () => new Lesson(title, content, invalidDescription!, course, 1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("A descrição não pode estar vazia*");
    }

    [Fact]
    public void Lesson_Constructor_WithNullCourse_ShouldThrowArgumentNullException()
    {
        // Arrange
        Course course = null!;
        var title = "Título válido";
        var content = "Conteúdo válido";
        var description = "Descrição válida";

        // Act & Assert
        var action = () => new Lesson(title, content, description, course, 1);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Lesson_Constructor_WithNegativeOrder_ShouldThrowArgumentException()
    {
        // Arrange
        var course = CreateValidCourse();
        var title = "Título válido";
        var content = "Conteúdo válido";
        var description = "Descrição válida";
        var order = -1;

        // Act & Assert
        var action = () => new Lesson(title, content, description, course, order);
        action.Should().Throw<ArgumentException>()
            .WithMessage("A ordem não pode ser negativa*");
    }

    [Fact]
    public void Lesson_Constructor_WithNegativeDuration_ShouldThrowArgumentException()
    {
        // Arrange
        var course = CreateValidCourse();
        var title = "Título válido";
        var content = "Conteúdo válido";
        var description = "Descrição válida";
        var order = 1;
        var duration = -10;

        // Act & Assert
        var action = () => new Lesson(title, content, description, course, order, duration);
        action.Should().Throw<ArgumentException>()
            .WithMessage("A duração não pode ser negativa*");
    }

    [Fact]
    public void UpdateMaterialUrl_ShouldUpdateMaterialUrl()
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título", "Conteúdo", "Descrição", course, 1);
        var materialUrl = "https://example.com/material.pdf";

        // Act
        lesson.UpdateMaterialUrl(materialUrl);

        // Assert
        lesson.MaterialUrl.Should().Be(materialUrl);
        lesson.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void IncrementCompletionCount_ShouldIncreaseCompletionCount()
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título", "Conteúdo", "Descrição", course, 1);
        var initialCount = lesson.CompletionCount;

        // Act
        lesson.IncrementCompletionCount();

        // Assert
        lesson.CompletionCount.Should().Be(initialCount + 1);
        lesson.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateContent_WithValidData_ShouldUpdateLessonContent()
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título inicial", "Conteúdo inicial", "Descrição inicial", course, 1, 10);
        var newTitle = "Título atualizado";
        var newContent = "Conteúdo atualizado";
        var newDescription = "Descrição atualizada";
        var newOrder = 2;
        var newDuration = 45;

        // Act
        lesson.UpdateContent(newTitle, newContent, newDescription, newOrder, newDuration);

        // Assert
        lesson.Title.Should().Be(newTitle);
        lesson.Content.Should().Be(newContent);
        lesson.Description.Should().Be(newDescription);
        lesson.Order.Should().Be(newOrder);
        lesson.DurationMinutes.Should().Be(newDuration);
        lesson.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateLesson()
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título inicial", "Conteúdo inicial", "Descrição inicial", course, 1, 10);
        var newTitle = "Título atualizado";
        var newDescription = "Descrição atualizada";
        var newContent = "Conteúdo atualizado";
        var materialUrl = "https://example.com/material.pdf";
        var newDuration = 45;

        // Act
        lesson.Update(newTitle, newDescription, newContent, materialUrl, newDuration);

        // Assert
        lesson.Title.Should().Be(newTitle);
        lesson.Description.Should().Be(newDescription);
        lesson.Content.Should().Be(newContent);
        lesson.MaterialUrl.Should().Be(materialUrl);
        lesson.DurationMinutes.Should().Be(newDuration);
        lesson.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateOrder_WithValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título", "Conteúdo", "Descrição", course, 1);
        var newOrder = 5;

        // Act
        lesson.UpdateOrder(newOrder);

        // Assert
        lesson.Order.Should().Be(newOrder);
        lesson.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateOrder_WithInvalidOrder_ShouldThrowArgumentException(int invalidOrder)
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título", "Conteúdo", "Descrição", course, 1);

        // Act & Assert
        var action = () => lesson.UpdateOrder(invalidOrder);
        action.Should().Throw<ArgumentException>()
            .WithMessage("A ordem deve ser positiva*");
    }

    [Fact]
    public void UpdateDuration_WithValidDuration_ShouldUpdateDuration()
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título", "Conteúdo", "Descrição", course, 1, 10);
        var newDuration = 60;

        // Act
        lesson.UpdateDuration(newDuration);

        // Assert
        lesson.DurationMinutes.Should().Be(newDuration);
        lesson.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDuration_WithNegativeDuration_ShouldThrowArgumentException()
    {
        // Arrange
        var course = CreateValidCourse();
        var lesson = new Lesson("Título", "Conteúdo", "Descrição", course, 1, 10);
        var invalidDuration = -5;

        // Act & Assert
        var action = () => lesson.UpdateDuration(invalidDuration);
        action.Should().Throw<ArgumentException>()
            .WithMessage("A duração não pode ser negativa*");
    }
} 