using FluencyHub.ContentManagement.Application.Commands.UpdateCourse;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using Moq;
using Xunit;
using FluentAssertions;
using AppInterfaces = FluencyHub.ContentManagement.Application.Common.Interfaces;

namespace FluencyHub.Tests.Unit.ContentManagement.Application.Commands;

public class UpdateCourseCommandHandlerTests
{
    private readonly Mock<AppInterfaces.ICourseRepository> _mockCourseRepository;
    private readonly UpdateCourseCommandHandler _handler;

    public UpdateCourseCommandHandlerTests()
    {
        _mockCourseRepository = new Mock<AppInterfaces.ICourseRepository>();
        _handler = new UpdateCourseCommandHandler(_mockCourseRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCourse_WhenValidCommand()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = new Course(
            "Curso Original",
            "Descrição original",
            new CourseContent("Syllabus", "Objetivos", "Pré-req", "Público", "Português", "Básico"),
            199.99m)
        {
            Name = "Curso Original",
            Description = "Descrição original",
            Content = new CourseContent("Syllabus", "Objetivos", "Pré-req", "Público", "Português", "Básico")
        };

        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Curso Atualizado",
            Description = "Descrição atualizada",
            Syllabus = "Novo syllabus",
            LearningObjectives = "Novos objetivos",
            PreRequisites = "Novos pré-requisitos",
            TargetAudience = "Novo público",
            Language = "Inglês",
            Level = "Intermediário",
            Price = 299.99m
        };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _mockCourseRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _mockCourseRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        
        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
        _mockCourseRepository.Verify(x => x.UpdateAsync(It.IsAny<Course>()), Times.Once);
        _mockCourseRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Curso Inexistente",
            Description = "Descrição",
            Syllabus = "Syllabus",
            LearningObjectives = "Objetivos",
            PreRequisites = "Pré-requisitos",
            TargetAudience = "Público",
            Language = "Português",
            Level = "Básico",
            Price = 199.99m
        };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(() => null!);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Curso com ID {courseId} não encontrado");
        
        _mockCourseRepository.Verify(x => x.UpdateAsync(It.IsAny<Course>()), Times.Never);
        _mockCourseRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHandleRepositoryException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = new Course(
            "Curso Original",
            "Descrição original",
            new CourseContent("Syllabus", "Objetivos", "Pré-req", "Público", "Português", "Básico"),
            199.99m)
        {
            Name = "Curso Original",
            Description = "Descrição original",
            Content = new CourseContent("Syllabus", "Objetivos", "Pré-req", "Público", "Português", "Básico")
        };

        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Curso Atualizado",
            Description = "Descrição atualizada",
            Syllabus = "Novo syllabus",
            LearningObjectives = "Novos objetivos",
            PreRequisites = "Novos pré-requisitos",
            TargetAudience = "Novo público",
            Language = "Inglês",
            Level = "Intermediário",
            Price = 299.99m
        };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _mockCourseRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Course>()))
            .ThrowsAsync(new InvalidOperationException("Erro no banco de dados"));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro no banco de dados");
    }
} 