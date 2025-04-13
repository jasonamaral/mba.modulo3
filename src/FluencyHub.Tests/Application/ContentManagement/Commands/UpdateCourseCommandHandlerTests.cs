using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Commands.UpdateCourse;
using FluencyHub.Domain.ContentManagement;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluencyHub.Tests.Application.ContentManagement.Commands;

public class UpdateCourseCommandHandlerTests
{
    private readonly Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ILogger<UpdateCourseCommandHandler>> _loggerMock;
    private readonly UpdateCourseCommandHandler _handler;
    
    public UpdateCourseCommandHandlerTests()
    {
        _courseRepositoryMock = new Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository>();
        _loggerMock = new Mock<ILogger<UpdateCourseCommandHandler>>();
        _handler = new UpdateCourseCommandHandler(_courseRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ValidCommand_UpdatesCourseSuccessfully()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Updated Course",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Advanced",
            Language = "Portuguese",
            Level = "C1",
            Price = 149.99m
        };
        
        // Create an existing course to be updated
        var courseContent = new CourseContent(
            "Original Syllabus",
            "Original Objectives",
            "Original Prerequisites",
            "Original Audience",
            "English",
            "B1");
            
        var existingCourse = new Course("Original Course", "Original Description", courseContent, 99.99m);
        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);
            
        _courseRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);
            
        _courseRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        
        _courseRepositoryMock.Verify(r => r.GetByIdAsync(courseId), Times.Once);
        _courseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Once);
        _courseRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_CourseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Updated Course",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Advanced",
            Language = "Portuguese",
            Level = "C1",
            Price = 149.99m
        };
        
        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync((Course)null);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None));
            
        Assert.Contains(courseId.ToString(), exception.Message);
        
        _courseRepositoryMock.Verify(r => r.GetByIdAsync(courseId), Times.Once);
        _courseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
        _courseRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new UpdateCourseCommand
        {
            Id = courseId,
            Name = "Updated Course",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Advanced",
            Language = "Portuguese",
            Level = "C1",
            Price = 149.99m
        };
        
        var courseContent = new CourseContent(
            "Original Syllabus",
            "Original Objectives",
            "Original Prerequisites",
            "Original Audience",
            "English",
            "B1");
            
        var existingCourse = new Course("Original Course", "Original Description", courseContent, 99.99m);
        
        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);
            
        var expectedException = new Exception("Database error");
        
        _courseRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Course>()))
            .Throws(expectedException);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(command, CancellationToken.None));
            
        Assert.Same(expectedException, exception);
        
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 