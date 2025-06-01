using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;
using FluentAssertions;

namespace FluencyHub.Tests.Unit.StudentManagement.Infrastructure;

public class LearningRepositoryTests : IDisposable
{
    private readonly StudentDbContext _context;
    private readonly LearningRepository _repository;

    public LearningRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StudentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StudentDbContext(options);
        _repository = new LearningRepository(_context);
    }

    [Fact]
    public async Task GetLearningHistoryByStudentIdAsync_ShouldReturnHistory_WhenExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLearningHistoryByStudentIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(studentId);
    }

    [Fact]
    public async Task GetLearningHistoryByStudentIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetLearningHistoryByStudentIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByStudentIdAsync_ShouldReturnHistory_WhenExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStudentIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(studentId);
    }

    [Fact]
    public async Task GetByStudentIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentStudentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByStudentIdAsync(nonExistentStudentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByStudentIdFreshAsync_ShouldReturnHistory_WhenExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStudentIdFreshAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(studentId);
    }

    [Fact]
    public async Task GetCourseProgressAsync_ShouldReturnProgress_WhenExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        
        learningHistory.AddCourseProgress(courseProgress);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCourseProgressAsync(courseId, studentId);

        // Assert
        result.Should().NotBeNull();
        result.CourseId.Should().Be(courseId);
        result.LearningHistoryId.Should().Be(studentId);
    }

    [Fact]
    public async Task GetCourseProgressAsync_ShouldThrowException_WhenNotExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var learningHistoryId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.GetCourseProgressAsync(courseId, learningHistoryId));
    }

    [Fact]
    public async Task GetAllCourseProgressAsync_ShouldReturnProgresses_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress1 = new CourseProgress(courseId1) { LearningHistoryId = studentId };
        var courseProgress2 = new CourseProgress(courseId2) { LearningHistoryId = studentId };
        
        learningHistory.AddCourseProgress(courseProgress1);
        learningHistory.AddCourseProgress(courseProgress2);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllCourseProgressAsync(studentId);

        // Assert
        var progresses = result.ToList();
        progresses.Should().HaveCount(2);
        progresses.Should().Contain(p => p.CourseId == courseId1);
        progresses.Should().Contain(p => p.CourseId == courseId2);
    }

    [Fact]
    public async Task GetAllCourseProgressAsync_ShouldReturnEmpty_WhenStudentNotExists()
    {
        // Arrange
        var nonExistentStudentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetAllCourseProgressAsync(nonExistentStudentId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddLearningHistoryAsync_ShouldAddHistory_WhenValidHistory()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);

        // Act
        await _repository.AddLearningHistoryAsync(learningHistory);
        await _repository.SaveChangesAsync();

        // Assert
        var savedHistory = await _context.LearningHistories.FindAsync(studentId);
        savedHistory.Should().NotBeNull();
        savedHistory!.Id.Should().Be(studentId);
    }

    [Fact]
    public async Task AddCourseProgressAsync_ShouldAddProgress_WhenValidProgress()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AddCourseProgressAsync(courseProgress);
        await _repository.SaveChangesAsync();

        // Assert
        var savedProgress = await _context.CourseProgresses
            .FirstOrDefaultAsync(cp => cp.CourseId == courseId && cp.LearningHistoryId == studentId);
        savedProgress.Should().NotBeNull();
        savedProgress!.CourseId.Should().Be(courseId);
    }

    [Fact]
    public async Task AddCompletedLessonAsync_ShouldAddLesson_WhenValidLesson()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        var completedLesson = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        
        learningHistory.AddCourseProgress(courseProgress);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AddCompletedLessonAsync(completedLesson);
        await _repository.SaveChangesAsync();

        // Assert
        var savedLesson = await _context.CompletedLessons
            .FirstOrDefaultAsync(cl => cl.LessonId == lessonId);
        savedLesson.Should().NotBeNull();
        savedLesson!.LessonId.Should().Be(lessonId);
    }

    [Fact]
    public async Task HasCompletedLessonAsync_ShouldReturnTrue_WhenLessonCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        var completedLesson = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        
        learningHistory.AddCourseProgress(courseProgress);
        courseProgress.AddCompletedLesson(completedLesson);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasCompletedLessonAsync(studentId, lessonId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasCompletedLessonAsync_ShouldReturnFalse_WhenLessonNotCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        // Act
        var result = await _repository.HasCompletedLessonAsync(studentId, lessonId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCompletedLessonsCountAsync_ShouldReturnCount_WhenLessonsExist()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId1 = Guid.NewGuid();
        var lessonId2 = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        var completedLesson1 = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId1,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        var completedLesson2 = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId2,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        
        learningHistory.AddCourseProgress(courseProgress);
        courseProgress.AddCompletedLesson(completedLesson1);
        courseProgress.AddCompletedLesson(completedLesson2);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompletedLessonsCountAsync(studentId, courseId);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCompletedLessonsCountAsync_ShouldReturnZero_WhenNoLessonsCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        // Act
        var result = await _repository.GetCompletedLessonsCountAsync(studentId, courseId);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetCompletedLessonIdsAsync_ShouldReturnIds_WhenLessonsExist()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId1 = Guid.NewGuid();
        var lessonId2 = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        var completedLesson1 = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId1,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        var completedLesson2 = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId2,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        
        learningHistory.AddCourseProgress(courseProgress);
        courseProgress.AddCompletedLesson(completedLesson1);
        courseProgress.AddCompletedLesson(completedLesson2);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompletedLessonIdsAsync(studentId, courseId);

        // Assert
        var lessonIds = result.ToList();
        lessonIds.Should().HaveCount(2);
        lessonIds.Should().Contain(lessonId1);
        lessonIds.Should().Contain(lessonId2);
    }

    [Fact]
    public async Task GetCompletedLessonIdsAsync_ShouldReturnEmpty_WhenNoLessonsCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        // Act
        var result = await _repository.GetCompletedLessonIdsAsync(studentId, courseId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteLessonAsync_ShouldCompleteLesson_WhenValidData()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        // Act
        await _repository.CompleteLessonAsync(studentId, courseId, lessonId);

        // Assert
        var hasCompleted = await _repository.HasCompletedLessonAsync(studentId, lessonId);
        hasCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteLessonAsync_ShouldNotDuplicate_WhenLessonAlreadyCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        // Act
        await _repository.CompleteLessonAsync(studentId, courseId, lessonId);
        await _repository.CompleteLessonAsync(studentId, courseId, lessonId); // Segunda tentativa

        // Assert
        var count = await _repository.GetCompletedLessonsCountAsync(studentId, courseId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task UncompleteLessonAsync_ShouldRemoveLesson_WhenLessonCompleted()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        await _repository.CompleteLessonAsync(studentId, courseId, lessonId);

        // Act
        await _repository.UncompleteLessonAsync(studentId, lessonId);

        // Assert
        var hasCompleted = await _repository.HasCompletedLessonAsync(studentId, lessonId);
        hasCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetCourseProgressByIdAsync_ShouldReturnProgress_WhenExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        
        learningHistory.AddCourseProgress(courseProgress);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCourseProgressByIdAsync(courseProgress.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(courseProgress.Id);
        result.CourseId.Should().Be(courseId);
    }

    [Fact]
    public async Task GetCourseProgressByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetCourseProgressByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCourseProgressesByStudentIdAsync_ShouldReturnProgresses_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress1 = new CourseProgress(courseId1) { LearningHistoryId = studentId };
        var courseProgress2 = new CourseProgress(courseId2) { LearningHistoryId = studentId };
        
        learningHistory.AddCourseProgress(courseProgress1);
        learningHistory.AddCourseProgress(courseProgress2);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCourseProgressesByStudentIdAsync(studentId);

        // Assert
        var progresses = result.ToList();
        progresses.Should().HaveCount(2);
        progresses.Should().Contain(p => p.CourseId == courseId1);
        progresses.Should().Contain(p => p.CourseId == courseId2);
    }

    [Fact]
    public async Task GetCompletedLessonsByCourseProgressIdAsync_ShouldReturnLessons_WhenLessonsExist()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId1 = Guid.NewGuid();
        var lessonId2 = Guid.NewGuid();
        var learningHistory = new LearningHistory(studentId);
        var courseProgress = new CourseProgress(courseId) { LearningHistoryId = studentId };
        var completedLesson1 = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId1,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        var completedLesson2 = new CompletedLesson
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId2,
            CompletedAt = DateTime.UtcNow,
            CourseProgressId = courseProgress.Id
        };
        
        learningHistory.AddCourseProgress(courseProgress);
        courseProgress.AddCompletedLesson(completedLesson1);
        courseProgress.AddCompletedLesson(completedLesson2);
        
        await _context.LearningHistories.AddAsync(learningHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompletedLessonsByCourseProgressIdAsync(courseProgress.Id);

        // Assert
        var lessons = result.ToList();
        lessons.Should().HaveCount(2);
        lessons.Should().Contain(l => l.LessonId == lessonId1);
        lessons.Should().Contain(l => l.LessonId == lessonId2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 