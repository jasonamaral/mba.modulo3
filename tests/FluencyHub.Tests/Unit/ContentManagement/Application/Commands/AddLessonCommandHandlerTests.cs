using FluentAssertions;
using FluencyHub.ContentManagement.Application.Commands.AddLesson;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;
using ILessonRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ILessonRepository;

namespace FluencyHub.Tests.Unit.ContentManagement.Application.Commands;

public class AddLessonCommandHandlerTests
{
    private readonly Mock<ILessonRepository> _mockLessonRepository;
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly Mock<ILogger<AddLessonCommandHandler>> _mockLogger;
    private readonly AddLessonCommandHandler _handler;

    public AddLessonCommandHandlerTests()
    {
        _mockLessonRepository = new Mock<ILessonRepository>();
        _mockCourseRepository = new Mock<ICourseRepository>();
        _mockLogger = new Mock<ILogger<AddLessonCommandHandler>>();
        _handler = new AddLessonCommandHandler(
            _mockLessonRepository.Object,
            _mockCourseRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateLessonAndReturnId()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = TestDataBuilder.CreateValidCourse();
        var command = new AddLessonCommand
        {
            CourseId = courseId,
            Title = "Lição de Teste",
            Description = "Descrição da lição",
            Content = "Conteúdo da lição",
            Order = 1,
            DurationMinutes = 30,
            VideoUrl = "https://example.com/video.mp4"
        };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        _mockLessonRepository
            .Setup(x => x.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockLessonRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
        _mockLessonRepository.Verify(x => x.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockLessonRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCourse_ShouldThrowNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new AddLessonCommand
        {
            CourseId = courseId,
            Title = "Lição de Teste",
            Description = "Descrição da lição",
            Content = "Conteúdo da lição",
            Order = 1,
            DurationMinutes = 30
        };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Course with ID {courseId} not found");
        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
        _mockLessonRepository.Verify(x => x.AddAsync(It.IsAny<Lesson>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLessonRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 