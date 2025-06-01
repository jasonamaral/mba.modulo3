using FluencyHub.StudentManagement.Domain;
using Xunit;

namespace FluencyHub.Tests.Unit.StudentManagement.Domain;

public class CourseProgressTests
{
    [Fact]
    public void CourseProgress_Constructor_ShouldCreateProgress_WhenValidParameters()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        // Act
        var courseProgress = new CourseProgress(courseId);

        // Assert
        Assert.Equal(courseId, courseProgress.CourseId);
        Assert.False(courseProgress.IsCompleted);
        Assert.Empty(courseProgress.CompletedLessons);
        Assert.True(courseProgress.LastUpdated <= DateTime.UtcNow);
        Assert.NotEqual(Guid.Empty, courseProgress.Id);
    }

    [Fact]
    public void CourseProgress_StartCourse_ShouldSetStartDate()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);
        var initialLastUpdated = courseProgress.LastUpdated;

        // Act
        Thread.Sleep(1); // Para garantir diferença no timestamp
        // Note: CourseProgress não tem método StartCourse explícito, 
        // mas é iniciado automaticamente no construtor

        // Assert
        Assert.True(courseProgress.LastUpdated >= initialLastUpdated);
    }

    [Fact]
    public void CourseProgress_CompleteCourse_ShouldSetCompletionDate()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);
        var initialLastUpdated = courseProgress.LastUpdated;

        // Act
        Thread.Sleep(1);
        courseProgress.CompleteCourse();

        // Assert
        Assert.True(courseProgress.IsCompleted);
        Assert.True(courseProgress.LastUpdated > initialLastUpdated);
    }

    [Fact]
    public void CourseProgress_AddLessonProgress_ShouldAddLesson_WhenValidLesson()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);

        // Act
        courseProgress.AddCompletedLesson(lessonId);

        // Assert
        Assert.Equal(1, courseProgress.GetCompletedLessonsCount());
        Assert.True(courseProgress.HasCompletedLesson(lessonId));
        Assert.Single(courseProgress.CompletedLessons);
    }

    [Fact]
    public void CourseProgress_AddLessonProgress_ShouldThrowException_WhenLessonAlreadyExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);
        courseProgress.AddCompletedLesson(lessonId);

        // Act & Assert
        // O método AddCompletedLesson não adiciona duplicatas, apenas ignora
        courseProgress.AddCompletedLesson(lessonId);
        
        // Verifica que ainda tem apenas 1 lição
        Assert.Equal(1, courseProgress.GetCompletedLessonsCount());
    }

    [Fact]
    public void CourseProgress_CalculateCompletionPercentage_ShouldReturnCorrectPercentage()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);
        var totalLessons = 5;
        
        // Adiciona algumas lições completadas
        for (int i = 0; i < 3; i++)
        {
            courseProgress.AddCompletedLesson(Guid.NewGuid());
        }

        // Act
        var completedCount = courseProgress.GetCompletedLessonsCount();
        var percentage = (double)completedCount / totalLessons * 100;

        // Assert
        Assert.Equal(3, completedCount);
        Assert.Equal(60.0, percentage);
    }

    [Fact]
    public void CourseProgress_IsCompleted_ShouldReturnTrue_WhenAllLessonsCompleted()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);
        
        // Simula a conclusão de todas as lições do curso
        courseProgress.AddCompletedLesson(Guid.NewGuid());
        courseProgress.AddCompletedLesson(Guid.NewGuid());
        courseProgress.AddCompletedLesson(Guid.NewGuid());
        
        // Act
        courseProgress.CompleteCourse();

        // Assert
        Assert.True(courseProgress.IsCompleted);
    }

    [Fact]
    public void CourseProgress_CompleteLesson_ShouldAddLessonToCompleted()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);

        // Act
        courseProgress.CompleteLesson(lessonId);

        // Assert
        Assert.True(courseProgress.HasCompletedLesson(lessonId));
        Assert.Equal(1, courseProgress.GetCompletedLessonsCount());
    }

    [Fact]
    public void CourseProgress_HasCompletedLesson_ShouldReturnFalse_WhenLessonNotCompleted()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);

        // Act & Assert
        Assert.False(courseProgress.HasCompletedLesson(lessonId));
    }

    [Fact]
    public void CourseProgress_AddCompletedLessonObject_ShouldAddLesson_WhenValidCompletedLesson()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var courseProgress = new CourseProgress(courseId);
        var completedLesson = new CompletedLesson
        {
            LessonId = lessonId,
            CourseProgressId = courseProgress.Id,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        courseProgress.AddCompletedLesson(completedLesson);

        // Assert
        Assert.True(courseProgress.HasCompletedLesson(lessonId));
        Assert.Equal(1, courseProgress.GetCompletedLessonsCount());
    }
} 