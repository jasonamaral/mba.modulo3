using FluencyHub.ContentManagement.Application.Commands.CreateCourse;
using FluencyHub.ContentManagement.Domain;
using Moq;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.ContentManagement.Application.Commands;

public class CreateCourseCommandHandlerTests
{
    private readonly Mock<FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository> _mockCourseRepository;
    private readonly CreateCourseCommandHandler _handler;

    public CreateCourseCommandHandlerTests()
    {
        _mockCourseRepository = new Mock<FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository>();
        _handler = new CreateCourseCommandHandler(_mockCourseRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCourseAndReturnId()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Curso de Inglês Básico",
            Description = "Curso completo de inglês para iniciantes",
            Syllabus = "Módulo 1: Alfabeto, Módulo 2: Números",
            LearningObjectives = "Aprender vocabulário básico",
            PreRequisites = "Nenhum",
            TargetAudience = "Iniciantes",
            Language = "Português",
            Level = "Básico",
            Price = 299.99m
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
            c.Name == command.Name &&
            c.Description == command.Description &&
            c.Price == command.Price)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldThrowException()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Curso de Inglês Básico",
            Description = "Curso completo de inglês para iniciantes",
            Syllabus = "Módulo 1: Alfabeto",
            LearningObjectives = "Aprender vocabulário básico",
            PreRequisites = "Nenhum",
            TargetAudience = "Iniciantes",
            Language = "Português",
            Level = "Básico",
            Price = 299.99m
        };

        _mockCourseRepository
            .Setup(x => x.AddAsync(It.IsAny<Course>()))
            .ThrowsAsync(new InvalidOperationException("Falha no repositório"));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Falha no repositório");
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