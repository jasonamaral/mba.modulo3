using FluencyHub.StudentManagement.Domain;
using Xunit;

namespace FluencyHub.Tests.Unit.StudentManagement.Domain;

public class CompletedLessonTests
{
    [Fact]
    public void CompletedLesson_Constructor_ShouldCreateProgress_WhenValidParameters()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var courseProgressId = Guid.NewGuid();
        var completedAt = DateTime.UtcNow;

        // Act
        var completedLesson = new CompletedLesson
        {
            LessonId = lessonId,
            CourseProgressId = courseProgressId,
            CompletedAt = completedAt
        };

        // Assert
        Assert.Equal(lessonId, completedLesson.LessonId);
        Assert.Equal(courseProgressId, completedLesson.CourseProgressId);
        Assert.Equal(completedAt, completedLesson.CompletedAt);
        Assert.NotEqual(Guid.Empty, completedLesson.Id);
    }

    [Fact]
    public void CompletedLesson_Constructor_ShouldGenerateUniqueId()
    {
        // Arrange & Act
        var completedLesson1 = new CompletedLesson
        {
            LessonId = Guid.NewGuid(),
            CourseProgressId = Guid.NewGuid(),
            CompletedAt = DateTime.UtcNow
        };

        var completedLesson2 = new CompletedLesson
        {
            LessonId = Guid.NewGuid(),
            CourseProgressId = Guid.NewGuid(),
            CompletedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(completedLesson1.Id, completedLesson2.Id);
        Assert.NotEqual(Guid.Empty, completedLesson1.Id);
        Assert.NotEqual(Guid.Empty, completedLesson2.Id);
    }

    [Fact]
    public void CompletedLesson_CompletedAt_ShouldAcceptValidDateTime()
    {
        // Arrange
        var specificDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var completedLesson = new CompletedLesson
        {
            LessonId = Guid.NewGuid(),
            CourseProgressId = Guid.NewGuid(),
            CompletedAt = specificDate
        };

        // Act & Assert
        Assert.Equal(specificDate, completedLesson.CompletedAt);
    }

    [Fact]
    public void CompletedLesson_CompletedAt_ShouldDefaultToCurrentTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var completedLesson = new CompletedLesson
        {
            LessonId = Guid.NewGuid(),
            CourseProgressId = Guid.NewGuid(),
            CompletedAt = DateTime.UtcNow
        };

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(completedLesson.CompletedAt >= beforeCreation);
        Assert.True(completedLesson.CompletedAt <= afterCreation);
    }

    [Fact]
    public void CompletedLesson_NavigationProperty_ShouldAllowNullCourseProgress()
    {
        // Arrange & Act
        var completedLesson = new CompletedLesson
        {
            LessonId = Guid.NewGuid(),
            CourseProgressId = Guid.NewGuid(),
            CompletedAt = DateTime.UtcNow,
            CourseProgress = null
        };

        // Assert
        Assert.Null(completedLesson.CourseProgress);
    }

    [Fact]
    public void CompletedLesson_NavigationProperty_ShouldAcceptCourseProgress()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);
        
        var completedLesson = new CompletedLesson
        {
            LessonId = Guid.NewGuid(),
            CourseProgressId = courseProgress.Id,
            CompletedAt = DateTime.UtcNow,
            CourseProgress = courseProgress
        };

        // Act & Assert
        Assert.NotNull(completedLesson.CourseProgress);
        Assert.Equal(courseProgress.Id, completedLesson.CourseProgress.Id);
    }

    [Fact]
    public void CompletedLesson_Properties_ShouldBeSettable()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var courseProgressId = Guid.NewGuid();
        var completedAt = DateTime.UtcNow;

        // Act
        var completedLesson = new CompletedLesson();
        completedLesson.LessonId = lessonId;
        completedLesson.CourseProgressId = courseProgressId;
        completedLesson.CompletedAt = completedAt;

        // Assert
        Assert.Equal(lessonId, completedLesson.LessonId);
        Assert.Equal(courseProgressId, completedLesson.CourseProgressId);
        Assert.Equal(completedAt, completedLesson.CompletedAt);
    }
} 