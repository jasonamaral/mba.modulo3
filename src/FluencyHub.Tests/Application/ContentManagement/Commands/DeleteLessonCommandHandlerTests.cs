using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Application.Commands.DeleteLesson;
using FluencyHub.ContentManagement.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluencyHub.Tests.Application.ContentManagement.Commands;

public class DeleteLessonCommandHandlerTests
{
    private readonly Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository> _mockCourseRepository;
    private readonly Mock<ILogger<DeleteLessonCommandHandler>> _mockLogger;
    private readonly DeleteLessonCommandHandler _handler;
    private readonly CancellationToken _cancellationToken;

    public DeleteLessonCommandHandlerTests()
    {
        _mockCourseRepository = new Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository>();
        _mockLogger = new Mock<ILogger<DeleteLessonCommandHandler>>();
        _handler = new DeleteLessonCommandHandler(_mockCourseRepository.Object, _mockLogger.Object);
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task Handle_WithValidCourseAndLesson_RemovesLesson()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        // Criando um curso e uma lição para o teste
        var courseContent = new CourseContent(
            "Test syllabus",
            "Test objectives",
            "No prerequisites",
            "Test audience",
            "English",
            "Intermediate");
            
        var course = new Course("Test Course", "Test description", courseContent, 99.99m);
        
        // Adicionando lição ao curso
        var lesson = course.AddLesson("Test Lesson", "Test Content", "http://example.com");
        
        // Definindo o ID da lição criada para o ID de teste
        var lessonToRemove = course.Lessons.First();
        typeof(Lesson).GetProperty("Id").SetValue(lessonToRemove, lessonId, null);

        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _mockCourseRepository.Setup(r => r.UpdateAsync(It.IsAny<Course>()))
            .Returns(Task.CompletedTask);

        _mockCourseRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteLessonCommand(courseId, lessonId);

        // Act
        await _handler.Handle(command, _cancellationToken);

        // Assert
        Assert.DoesNotContain(course.Lessons, l => l.Id == lessonId);
        _mockCourseRepository.Verify(r => r.UpdateAsync(course), Times.Once);
        _mockCourseRepository.Verify(r => r.SaveChangesAsync(_cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCourse_ThrowsNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync((Course)null);

        var command = new DeleteLessonCommand(courseId, lessonId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, _cancellationToken));
        
        Assert.Equal("Course", exception.Message.Split('"')[1]);
        
        _mockCourseRepository.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
        _mockCourseRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentLesson_ThrowsNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var nonExistentLessonId = Guid.NewGuid();
        
        // Criando um curso e uma lição para o teste
        var courseContent = new CourseContent(
            "Test syllabus",
            "Test objectives",
            "No prerequisites",
            "Test audience",
            "English",
            "Intermediate");
            
        var course = new Course("Test Course", "Test description", courseContent, 99.99m);
        
        // Adicionando lição ao curso
        var lesson = course.AddLesson("Test Lesson", "Test Content", "http://example.com");
        
        // Definindo o ID da lição criada para o ID de teste
        var existingLesson = course.Lessons.First();
        typeof(Lesson).GetProperty("Id").SetValue(existingLesson, lessonId, null);

        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var command = new DeleteLessonCommand(courseId, nonExistentLessonId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, _cancellationToken));
        
        Assert.Equal("Lesson", exception.Message.Split('"')[1]);
        
        _mockCourseRepository.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
        _mockCourseRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 