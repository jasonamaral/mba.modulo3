using FluencyHub.ContentManagement.Application.Commands.CreateCourse;
using FluencyHub.ContentManagement.Domain;
using Moq;
using Xunit;
using FluentAssertions;
using AppInterfaces = FluencyHub.ContentManagement.Application.Common.Interfaces;

namespace FluencyHub.Tests.Unit.ContentManagement.Application.Commands;

public class CreateCourseCommandHandlerTests
{
    private readonly Mock<AppInterfaces.ICourseRepository> _mockCourseRepository;
    private readonly CreateCourseCommandHandler _handler;

    public CreateCourseCommandHandlerTests()
    {
        _mockCourseRepository = new Mock<AppInterfaces.ICourseRepository>();
        _handler = new CreateCourseCommandHandler(_mockCourseRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCourse_WhenValidCommand()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Curso de Inglês Básico",
            Description = "Curso completo de inglês para iniciantes",
            Syllabus = "Gramática básica, vocabulário essencial",
            LearningObjectives = "Comunicação básica em inglês",
            PreRequisites = "Nenhum",
            TargetAudience = "Iniciantes",
            Language = "Inglês",
            Level = "Básico",
            Price = 299.99m
        };

        var createdCourse = new Course(
            command.Name,
            command.Description,
            new CourseContent(
                command.Syllabus,
                command.LearningObjectives,
                command.PreRequisites,
                command.TargetAudience,
                command.Language,
                command.Level),
            command.Price)
        {
            Name = command.Name,
            Description = command.Description,
            Content = new CourseContent(
                command.Syllabus,
                command.LearningObjectives,
                command.PreRequisites,
                command.TargetAudience,
                command.Language,
                command.Level)
        };

        _mockCourseRepository
            .Setup(x => x.AddAsync(It.IsAny<Course>()))
            .Returns(Task.FromResult(createdCourse));

        _mockCourseRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        _mockCourseRepository.Verify(x => x.AddAsync(It.IsAny<Course>()), Times.Once);
        _mockCourseRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateCourseWithCorrectProperties_WhenValidCommand()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Curso de Espanhol",
            Description = "Aprenda espanhol do zero",
            Syllabus = "Verbos, substantivos, adjetivos",
            LearningObjectives = "Conversação fluente",
            PreRequisites = "Ensino médio completo",
            TargetAudience = "Adultos",
            Language = "Espanhol",
            Level = "Intermediário",
            Price = 399.99m
        };

        Course capturedCourse = null!;
        _mockCourseRepository
            .Setup(x => x.AddAsync(It.IsAny<Course>()))
            .Callback<Course>(course => capturedCourse = course)
            .Returns(Task.FromResult(It.IsAny<Course>()));

        _mockCourseRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        capturedCourse.Should().NotBeNull();
        capturedCourse.Name.Should().Be(command.Name);
        capturedCourse.Description.Should().Be(command.Description);
        capturedCourse.Price.Should().Be(command.Price);
        capturedCourse.Content.Language.Should().Be(command.Language);
        capturedCourse.Content.Level.Should().Be(command.Level);
    }

    [Fact]
    public async Task Handle_ShouldHandleRepositoryException()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Curso de Francês",
            Description = "Francês para turismo",
            Syllabus = "Frases básicas",
            LearningObjectives = "Comunicação turística",
            PreRequisites = "Nenhum",
            TargetAudience = "Turistas",
            Language = "Francês",
            Level = "Básico",
            Price = 199.99m
        };

        _mockCourseRepository
            .Setup(x => x.AddAsync(It.IsAny<Course>()))
            .ThrowsAsync(new InvalidOperationException("Erro no banco de dados"));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro no banco de dados");
    }

    [Theory]
    [InlineData("", "Descrição válida", 100)]
    [InlineData("Nome válido", "", 100)]
    [InlineData("Nome válido", "Descrição válida", -10)]
    public async Task Handle_WithInvalidData_ShouldThrowArgumentException(string name, string description, decimal price)
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = name,
            Description = description,
            Syllabus = "Módulo 1",
            LearningObjectives = "Objetivos",
            PreRequisites = "Nenhum",
            TargetAudience = "Todos",
            Language = "Português",
            Level = "Básico",
            Price = price
        };

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WithZeroPrice_ShouldCreateFreeCourse()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Curso Gratuito",
            Description = "Curso gratuito de introdução",
            Syllabus = "Módulo 1: Introdução",
            LearningObjectives = "Conhecimentos básicos",
            PreRequisites = "Nenhum",
            TargetAudience = "Todos",
            Language = "Português",
            Level = "Básico",
            Price = 0m
        };

        var courseId = Guid.NewGuid();

        _mockCourseRepository
            .Setup(x => x.AddAsync(It.IsAny<Course>()))
            .ReturnsAsync((Course course) => 
            {
                // Simula a definição do ID pelo repositório
                typeof(Course).GetProperty("Id")?.SetValue(course, courseId);
                return course;
            });

        _mockCourseRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(courseId);
        _mockCourseRepository.Verify(x => x.AddAsync(It.Is<Course>(c => 
            c.Price == 0m)), Times.Once);
    }
} 