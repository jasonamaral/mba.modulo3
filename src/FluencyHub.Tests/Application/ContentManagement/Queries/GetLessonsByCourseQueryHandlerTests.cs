using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Queries.GetLessonsByCourse;
using FluencyHub.Domain.ContentManagement;
using Moq;

namespace FluencyHub.Tests.Application.ContentManagement.Queries;

public class GetLessonsByCourseQueryHandlerTests
{
    private readonly Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository> _courseRepositoryMock;
    private readonly GetLessonsByCourseQueryHandler _handler;
    
    public GetLessonsByCourseQueryHandlerTests()
    {
        _courseRepositoryMock = new Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository>();
        _handler = new GetLessonsByCourseQueryHandler(_courseRepositoryMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithExistingCourseId_ReturnsLessonsList()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course(
            "Test Course", 
            "Test Description", 
            new CourseContent(
                "Test Syllabus", 
                "Test Objectives", 
                "Test Prerequisites", 
                "Test Audience", 
                "English", 
                "Beginner"), 
            99.99m);
        
        // Set the Id property using reflection (since it's readonly)
        typeof(Course).GetProperty("Id").SetValue(course, courseId);

        var lesson1 = course.AddLesson("Lesson 1", "Content 1", null);
        var lesson2 = course.AddLesson("Lesson 2", "Content 2", "material.pdf");
        
        // Set lesson ids
        typeof(Lesson).GetProperty("Id").SetValue(lesson1, Guid.NewGuid());
        typeof(Lesson).GetProperty("Id").SetValue(lesson2, Guid.NewGuid());

        _courseRepositoryMock.Setup(r => r.GetByIdWithLessonsAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(new GetLessonsByCourseQuery(courseId), CancellationToken.None);

        // Assert
        var lessons = result.ToList();
        Assert.Equal(2, lessons.Count);
        
        Assert.Equal("Lesson 1", lessons[0].Title);
        Assert.Equal("Content 1", lessons[0].Content);
        Assert.Null(lessons[0].MaterialUrl);
        Assert.Equal(1, lessons[0].Order);
        
        Assert.Equal("Lesson 2", lessons[1].Title);
        Assert.Equal("Content 2", lessons[1].Content);
        Assert.Equal("material.pdf", lessons[1].MaterialUrl);
        Assert.Equal(2, lessons[1].Order);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingCourseId_ThrowsNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _courseRepositoryMock.Setup(r => r.GetByIdWithLessonsAsync(courseId))
            .ReturnsAsync((Course)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetLessonsByCourseQuery(courseId), CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithExistingCourseIdButNoLessons_ReturnsEmptyList()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course(
            "Test Course", 
            "Test Description", 
            new CourseContent(
                "Test Syllabus", 
                "Test Objectives", 
                "Test Prerequisites", 
                "Test Audience", 
                "English", 
                "Beginner"), 
            99.99m);
        
        // Set the Id property using reflection (since it's readonly)
        typeof(Course).GetProperty("Id").SetValue(course, courseId);

        _courseRepositoryMock.Setup(r => r.GetByIdWithLessonsAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(new GetLessonsByCourseQuery(courseId), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
} 