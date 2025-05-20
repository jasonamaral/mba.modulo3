using FluencyHub.ContentManagement.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Application.Common.Models;
using FluencyHub.ContentManagement.Application.Queries.GetLessonsByCourse;
using FluencyHub.ContentManagement.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluencyHub.Tests.Unit.ContextoCursos;

public class GetLessonsByCourseQueryHandlerTests
{
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly GetLessonsByCourseQueryHandler _handler;

    public GetLessonsByCourseQueryHandlerTests()
    {
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _handler = new GetLessonsByCourseQueryHandler(_lessonRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLessons_WhenLessonsExist()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var query = new GetLessonsByCourseQuery { CourseId = courseId };
        var lessons = new List<Lesson>
        {
            CreateLesson(courseId, "Lesson 1", "Content 1", 1),
            CreateLesson(courseId, "Lesson 2", "Content 2", 2)
        };

        _lessonRepositoryMock.Setup(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        var lessonsList = result.ToList();
        Assert.Equal("Lesson 1", lessonsList[0].Title);
        Assert.Equal("Content 1", lessonsList[0].Content);
        Assert.Equal(1, lessonsList[0].Order);
        Assert.Equal("Lesson 2", lessonsList[1].Title);
        Assert.Equal("Content 2", lessonsList[1].Content);
        Assert.Equal(2, lessonsList[1].Order);

        _lessonRepositoryMock.Verify(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoLessonsExist()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var query = new GetLessonsByCourseQuery { CourseId = courseId };
        var lessons = new List<Lesson>();

        _lessonRepositoryMock.Setup(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _lessonRepositoryMock.Verify(x => x.GetByCourseIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private Lesson CreateLesson(Guid courseId, string title, string content, int order)
    {
        var lesson = new Lesson(title, content, null, order);
        var lessonType = typeof(Lesson);
        var courseIdProperty = lessonType.GetProperty("CourseId");
        courseIdProperty?.SetValue(lesson, courseId);
        return lesson;
    }
} 