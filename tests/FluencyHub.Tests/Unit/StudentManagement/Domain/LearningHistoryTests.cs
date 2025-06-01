using FluentAssertions;
using FluencyHub.StudentManagement.Domain;
using Xunit;

namespace FluencyHub.Tests.Unit.StudentManagement.Domain;

public class LearningHistoryTests
{
    [Fact]
    public void LearningHistory_Constructor_ShouldCreateValidLearningHistory()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Act
        var learningHistory = new LearningHistory(studentId);

        // Assert
        learningHistory.StudentId.Should().Be(studentId);
        learningHistory.Id.Should().Be(studentId);
        learningHistory.CourseProgresses.Should().BeEmpty();
        learningHistory.Records.Should().BeEmpty();
        learningHistory.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        learningHistory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddProgress_WithNewCourse_ShouldCreateCourseProgressAndAddLesson()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        learningHistory.AddProgress(courseId, lessonId);

        // Assert
        learningHistory.CourseProgresses.Should().HaveCount(1);
        learningHistory.Records.Should().HaveCount(1);
        
        var courseProgress = learningHistory.CourseProgresses.First();
        courseProgress.CourseId.Should().Be(courseId);
        courseProgress.LearningHistoryId.Should().Be(studentId);
        
        var record = learningHistory.Records.First();
        record.LessonId.Should().Be(lessonId);
    }

    [Fact]
    public void AddProgress_WithExistingCourse_ShouldAddLessonToExistingProgress()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId1 = Guid.NewGuid();
        var lessonId2 = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        learningHistory.AddProgress(courseId, lessonId1);
        learningHistory.AddProgress(courseId, lessonId2);

        // Assert
        learningHistory.CourseProgresses.Should().HaveCount(1);
        learningHistory.Records.Should().HaveCount(2);
        
        var courseProgress = learningHistory.CourseProgresses.First();
        courseProgress.GetCompletedLessonsCount().Should().Be(2);
    }

    [Fact]
    public void AddLearningRecord_WithNewLesson_ShouldAddRecord()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var grade = 8.5f;
        var learningHistory = new LearningHistory(studentId);

        // Act
        learningHistory.AddLearningRecord(lessonId, grade);

        // Assert
        learningHistory.Records.Should().HaveCount(1);
        
        var record = learningHistory.Records.First();
        record.LessonId.Should().Be(lessonId);
        record.Grade.Should().Be(grade);
    }

    [Fact]
    public void AddLearningRecord_WithDuplicateLesson_ShouldNotAddDuplicate()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        learningHistory.AddLearningRecord(lessonId, 8.0f);
        learningHistory.AddLearningRecord(lessonId, 9.0f); // Tentativa de duplicar

        // Assert
        learningHistory.Records.Should().HaveCount(1);
        learningHistory.Records.First().Grade.Should().Be(8.0f); // Mantém o primeiro
    }

    [Fact]
    public void GetRecord_WithExistingLesson_ShouldReturnRecord()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var grade = 7.5f;
        var learningHistory = new LearningHistory(studentId);
        learningHistory.AddLearningRecord(lessonId, grade);

        // Act
        var record = learningHistory.GetRecord(lessonId);

        // Assert
        record.Should().NotBeNull();
        record!.LessonId.Should().Be(lessonId);
        record.Grade.Should().Be(grade);
    }

    [Fact]
    public void GetRecord_WithNonExistingLesson_ShouldReturnNull()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        var record = learningHistory.GetRecord(lessonId);

        // Assert
        record.Should().BeNull();
    }

    [Fact]
    public void CompleteCourse_WithNewCourse_ShouldCreateCourseProgressAndMarkAsCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        learningHistory.CompleteCourse(courseId);

        // Assert
        learningHistory.CourseProgresses.Should().HaveCount(1);
        
        var courseProgress = learningHistory.CourseProgresses.First();
        courseProgress.CourseId.Should().Be(courseId);
        courseProgress.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void CompleteCourse_WithExistingCourse_ShouldMarkExistingProgressAsCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        learningHistory.AddProgress(courseId, lessonId);

        // Act
        learningHistory.CompleteCourse(courseId);

        // Assert
        learningHistory.CourseProgresses.Should().HaveCount(1);
        
        var courseProgress = learningHistory.CourseProgresses.First();
        courseProgress.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void HasCompletedLesson_WithCompletedLesson_ShouldReturnTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        learningHistory.AddProgress(courseId, lessonId);

        // Act
        var hasCompleted = learningHistory.HasCompletedLesson(courseId, lessonId);

        // Assert
        hasCompleted.Should().BeTrue();
    }

    [Fact]
    public void HasCompletedLesson_WithNonCompletedLesson_ShouldReturnFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        var hasCompleted = learningHistory.HasCompletedLesson(courseId, lessonId);

        // Assert
        hasCompleted.Should().BeFalse();
    }

    [Fact]
    public void HasCompletedCourse_WithCompletedCourse_ShouldReturnTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        learningHistory.CompleteCourse(courseId);

        // Act
        var hasCompleted = learningHistory.HasCompletedCourse(courseId);

        // Assert
        hasCompleted.Should().BeTrue();
    }

    [Fact]
    public void HasCompletedCourse_WithNonCompletedCourse_ShouldReturnFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        var hasCompleted = learningHistory.HasCompletedCourse(courseId);

        // Assert
        hasCompleted.Should().BeFalse();
    }

    [Fact]
    public void GetCompletedLessonsCount_WithCompletedLessons_ShouldReturnCorrectCount()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId1 = Guid.NewGuid();
        var lessonId2 = Guid.NewGuid();
        var lessonId3 = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        
        learningHistory.AddProgress(courseId, lessonId1);
        learningHistory.AddProgress(courseId, lessonId2);
        learningHistory.AddProgress(courseId, lessonId3);

        // Act
        var count = learningHistory.GetCompletedLessonsCount(courseId);

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public void GetCompletedLessonsCount_WithNonExistingCourse_ShouldReturnZero()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        var count = learningHistory.GetCompletedLessonsCount(courseId);

        // Assert
        count.Should().Be(0);
    }
} 