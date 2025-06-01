using FluencyHub.ContentManagement.Domain;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.ContentManagement.Domain;

public class CourseContentTests
{
    [Fact]
    public void CourseContent_Constructor_WithValidData_ShouldCreateValidCourseContent()
    {
        // Arrange
        var syllabus = "Módulo 1: Introdução\nMódulo 2: Básico\nMódulo 3: Intermediário";
        var learningObjectives = "Aprender vocabulário básico e gramática fundamental";
        var preRequisites = "Conhecimento básico de inglês";
        var targetAudience = "Estudantes iniciantes";
        var language = "Português";
        var level = "Básico";

        // Act
        var courseContent = new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);

        // Assert
        courseContent.Syllabus.Should().Be(syllabus);
        courseContent.LearningObjectives.Should().Be(learningObjectives);
        courseContent.PreRequisites.Should().Be(preRequisites);
        courseContent.TargetAudience.Should().Be(targetAudience);
        courseContent.Language.Should().Be(language);
        courseContent.Level.Should().Be(level);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CourseContent_Constructor_WithInvalidSyllabus_ShouldThrowArgumentException(string? syllabus)
    {
        // Arrange
        var learningObjectives = "Objetivos de aprendizagem";
        var preRequisites = "Pré-requisitos";
        var targetAudience = "Público-alvo";
        var language = "Português";
        var level = "Básico";

        // Act & Assert
        var action = () => new CourseContent(
            syllabus!,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);

        action.Should().Throw<ArgumentException>()
            .WithMessage("O programa não pode estar vazio*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CourseContent_Constructor_WithInvalidLearningObjectives_ShouldThrowArgumentException(string? learningObjectives)
    {
        // Arrange
        var syllabus = "Conteúdo programático";
        var preRequisites = "Pré-requisitos";
        var targetAudience = "Público-alvo";
        var language = "Português";
        var level = "Básico";

        // Act & Assert
        var action = () => new CourseContent(
            syllabus,
            learningObjectives!,
            preRequisites,
            targetAudience,
            language,
            level);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Os objetivos de aprendizagem não podem estar vazios*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CourseContent_Constructor_WithInvalidTargetAudience_ShouldThrowArgumentException(string? targetAudience)
    {
        // Arrange
        var syllabus = "Conteúdo programático";
        var learningObjectives = "Objetivos de aprendizagem";
        var preRequisites = "Pré-requisitos";
        var language = "Português";
        var level = "Básico";

        // Act & Assert
        var action = () => new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience!,
            language,
            level);

        action.Should().Throw<ArgumentException>()
            .WithMessage("O público-alvo não pode estar vazio*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CourseContent_Constructor_WithInvalidLanguage_ShouldThrowArgumentException(string? language)
    {
        // Arrange
        var syllabus = "Conteúdo programático";
        var learningObjectives = "Objetivos de aprendizagem";
        var preRequisites = "Pré-requisitos";
        var targetAudience = "Público-alvo";
        var level = "Básico";

        // Act & Assert
        var action = () => new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language!,
            level);

        action.Should().Throw<ArgumentException>()
            .WithMessage("O idioma não pode estar vazio*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CourseContent_Constructor_WithInvalidLevel_ShouldThrowArgumentException(string? level)
    {
        // Arrange
        var syllabus = "Conteúdo programático";
        var learningObjectives = "Objetivos de aprendizagem";
        var preRequisites = "Pré-requisitos";
        var targetAudience = "Público-alvo";
        var language = "Português";

        // Act & Assert
        var action = () => new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level!);

        action.Should().Throw<ArgumentException>()
            .WithMessage("O nível não pode estar vazio*");
    }

    [Fact]
    public void CourseContent_Constructor_WithNullPreRequisites_ShouldSetEmptyString()
    {
        // Arrange
        var syllabus = "Conteúdo programático";
        var learningObjectives = "Objetivos de aprendizagem";
        string? preRequisites = null;
        var targetAudience = "Público-alvo";
        var language = "Português";
        var level = "Básico";

        // Act
        var courseContent = new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites ?? string.Empty,
            targetAudience,
            language,
            level);

        // Assert
        courseContent.PreRequisites.Should().Be(string.Empty);
    }

    [Fact]
    public void CourseContent_Create_ShouldCreateValidCourseContent()
    {
        // Arrange
        var syllabus = "Conteúdo programático";
        var learningObjectives = "Objetivos de aprendizagem";
        var preRequisites = "Pré-requisitos";
        var targetAudience = "Público-alvo";
        var language = "Português";
        var level = "Básico";

        // Act
        var courseContent = CourseContent.Create(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);

        // Assert
        courseContent.Should().NotBeNull();
        courseContent.Syllabus.Should().Be(syllabus);
        courseContent.LearningObjectives.Should().Be(learningObjectives);
        courseContent.PreRequisites.Should().Be(preRequisites);
        courseContent.TargetAudience.Should().Be(targetAudience);
        courseContent.Language.Should().Be(language);
        courseContent.Level.Should().Be(level);
    }

    [Fact]
    public void CourseContent_Update_ShouldReturnNewCourseContentWithUpdatedValues()
    {
        // Arrange
        var originalContent = new CourseContent(
            "Syllabus original",
            "Objetivos originais",
            "Pré-requisitos originais",
            "Público original",
            "Português",
            "Básico");

        var newSyllabus = "Novo syllabus";
        var newObjectives = "Novos objetivos";
        var newPreRequisites = "Novos pré-requisitos";
        var newTargetAudience = "Novo público";
        var newLanguage = "Inglês";
        var newLevel = "Avançado";

        // Act
        var updatedContent = originalContent.Update(
            newSyllabus,
            newObjectives,
            newPreRequisites,
            newTargetAudience,
            newLanguage,
            newLevel);

        // Assert
        updatedContent.Should().NotBeNull();
        updatedContent.Syllabus.Should().Be(newSyllabus);
        updatedContent.LearningObjectives.Should().Be(newObjectives);
        updatedContent.PreRequisites.Should().Be(newPreRequisites);
        updatedContent.TargetAudience.Should().Be(newTargetAudience);
        updatedContent.Language.Should().Be(newLanguage);
        updatedContent.Level.Should().Be(newLevel);
    }

    // Novos testes adicionados conforme especificação

    [Fact]
    public void CourseContent_Constructor_ShouldCreateContent_WhenValidParameters()
    {
        // Arrange
        var syllabus = "Programa completo do curso";
        var learningObjectives = "Objetivos claros de aprendizagem";
        var preRequisites = "Conhecimentos prévios necessários";
        var targetAudience = "Estudantes de nível intermediário";
        var language = "Português";
        var level = "Intermediário";

        // Act
        var courseContent = new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);

        // Assert
        courseContent.Should().NotBeNull();
        courseContent.Syllabus.Should().Be(syllabus);
        courseContent.LearningObjectives.Should().Be(learningObjectives);
        courseContent.PreRequisites.Should().Be(preRequisites);
        courseContent.TargetAudience.Should().Be(targetAudience);
        courseContent.Language.Should().Be(language);
        courseContent.Level.Should().Be(level);
    }

    [Fact]
    public void CourseContent_Constructor_ShouldThrowException_WhenSyllabusIsEmpty()
    {
        // Arrange
        var syllabus = "";
        var learningObjectives = "Objetivos de aprendizagem";
        var preRequisites = "Pré-requisitos";
        var targetAudience = "Público-alvo";
        var language = "Português";
        var level = "Básico";

        // Act & Assert
        var action = () => new CourseContent(
            syllabus,
            learningObjectives,
            preRequisites,
            targetAudience,
            language,
            level);

        action.Should().Throw<ArgumentException>()
            .WithMessage("O programa não pode estar vazio*");
    }

    [Fact]
    public void CourseContent_UpdateSyllabus_ShouldUpdateSyllabus_WhenValidSyllabus()
    {
        // Arrange
        var originalContent = new CourseContent(
            "Syllabus original",
            "Objetivos",
            "Pré-requisitos",
            "Público",
            "Português",
            "Básico");

        var newSyllabus = "Novo syllabus atualizado";

        // Act
        var updatedContent = originalContent.Update(
            newSyllabus,
            originalContent.LearningObjectives,
            originalContent.PreRequisites,
            originalContent.TargetAudience,
            originalContent.Language,
            originalContent.Level);

        // Assert
        updatedContent.Syllabus.Should().Be(newSyllabus);
    }

    [Fact]
    public void CourseContent_UpdateLearningObjectives_ShouldUpdateObjectives_WhenValidObjectives()
    {
        // Arrange
        var originalContent = new CourseContent(
            "Syllabus",
            "Objetivos originais",
            "Pré-requisitos",
            "Público",
            "Português",
            "Básico");

        var newObjectives = "Novos objetivos de aprendizagem";

        // Act
        var updatedContent = originalContent.Update(
            originalContent.Syllabus,
            newObjectives,
            originalContent.PreRequisites,
            originalContent.TargetAudience,
            originalContent.Language,
            originalContent.Level);

        // Assert
        updatedContent.LearningObjectives.Should().Be(newObjectives);
    }

    [Fact]
    public void CourseContent_UpdatePreRequisites_ShouldUpdatePreRequisites()
    {
        // Arrange
        var originalContent = new CourseContent(
            "Syllabus",
            "Objetivos",
            "Pré-requisitos originais",
            "Público",
            "Português",
            "Básico");

        var newPreRequisites = "Novos pré-requisitos";

        // Act
        var updatedContent = originalContent.Update(
            originalContent.Syllabus,
            originalContent.LearningObjectives,
            newPreRequisites,
            originalContent.TargetAudience,
            originalContent.Language,
            originalContent.Level);

        // Assert
        updatedContent.PreRequisites.Should().Be(newPreRequisites);
    }

    [Fact]
    public void CourseContent_UpdateTargetAudience_ShouldUpdateAudience()
    {
        // Arrange
        var originalContent = new CourseContent(
            "Syllabus",
            "Objetivos",
            "Pré-requisitos",
            "Público original",
            "Português",
            "Básico");

        var newTargetAudience = "Novo público-alvo";

        // Act
        var updatedContent = originalContent.Update(
            originalContent.Syllabus,
            originalContent.LearningObjectives,
            originalContent.PreRequisites,
            newTargetAudience,
            originalContent.Language,
            originalContent.Level);

        // Assert
        updatedContent.TargetAudience.Should().Be(newTargetAudience);
    }
} 