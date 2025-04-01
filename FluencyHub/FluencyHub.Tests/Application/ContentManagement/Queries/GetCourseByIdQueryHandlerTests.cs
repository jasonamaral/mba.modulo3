using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Domain.ContentManagement;
using Moq;

namespace FluencyHub.Tests.Application.ContentManagement.Queries;

public class GetCourseByIdQueryHandlerTests
{
    private readonly Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository> _courseRepositoryMock;
    private readonly GetCourseByIdQueryHandler _handler;
    
    public GetCourseByIdQueryHandlerTests()
    {
        _courseRepositoryMock = new Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository>();
        _handler = new GetCourseByIdQueryHandler(_courseRepositoryMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithExistingId_ReturnsCourseDto()
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

        _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(new GetCourseByIdQuery(courseId), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(courseId, result.Id);
        Assert.Equal("Test Course", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("Test Syllabus", result.Content.Syllabus);
        Assert.Equal("Test Objectives", result.Content.LearningObjectives);
        Assert.Equal("Test Prerequisites", result.Content.PreRequisites);
        Assert.Equal("Test Audience", result.Content.TargetAudience);
        Assert.Equal("English", result.Content.Language);
        Assert.Equal("Beginner", result.Content.Level);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _courseRepositoryMock.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync((Course)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetCourseByIdQuery(courseId), CancellationToken.None));
    }
} 