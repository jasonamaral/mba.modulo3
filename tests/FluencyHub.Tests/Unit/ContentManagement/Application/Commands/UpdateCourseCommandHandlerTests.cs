using FluentAssertions;
using FluencyHub.ContentManagement.Application.Commands.UpdateCourse;
using FluencyHub.ContentManagement.Application.Common.Exceptions;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;

namespace FluencyHub.Tests.Unit.ContentManagement.Application.Commands;

public class UpdateCourseCommandHandlerTests
{
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly Mock<ILogger<UpdateCourseCommandHandler>> _mockLogger;
    private readonly UpdateCourseCommandHandler _handler;

    public UpdateCourseCommandHandlerTests()
    {
        _mockCourseRepository = new Mock<ICourseRepository>();
        _mockLogger = new Mock<ILogger<UpdateCourseCommandHandler>>();
        _handler = new UpdateCourseCommandHandler(_mockCourseRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateCourseAndReturnTrue()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = TestDataBuilder.CreateValidCourse();
        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Updated Course Name",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Learning Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Updated Target Audience",
            Language = "Updated Language",
            Level = "Updated Level",
            Price = 199.99m
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
    public async Task Handle_WithNonExistentCourse_ShouldThrowNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Updated Course Name",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Learning Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Updated Target Audience",
            Language = "Updated Language",
            Level = "Updated Level",
            Price = 199.99m
        };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Course with ID {courseId} not found");
        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
        _mockCourseRepository.Verify(x => x.UpdateAsync(It.IsAny<Course>()), Times.Never);
        _mockCourseRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 