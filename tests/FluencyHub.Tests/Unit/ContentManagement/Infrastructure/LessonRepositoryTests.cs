using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;
using FluencyHub.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FluencyHub.Tests.Unit.ContentManagement.Infrastructure;

public class LessonRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly Mock<IDomainEventService> _mockEventService;
    private readonly LessonRepository _repository;
    private readonly DbContextOptions<ContentDbContext> _options;

    public LessonRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(_options);
        _mockEventService = new Mock<IDomainEventService>();
        _repository = new LessonRepository(_context, _mockEventService.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLesson_WhenLessonExists()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        var courseContent = new CourseContent(
            "Syllabus", 
            "Learning Objectives", 
            "Prerequisites", 
            "Target Audience", 
            "Portuguese", 
            "Beginner"
        );
        
        var course = new Course("Test Course", "Test Description", courseContent, 100m)
        {
            Name = "Test Course",
            Description = "Test Description",
            Content = courseContent
        };
        
        course.AddLesson("Test Lesson", "Test Content", "Test Description", 1, 30);

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var lesson = course.Lessons.First();

        // Act
        var result = await repository.GetByIdAsync(lesson.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lesson.Title, result.Title);
        Assert.Equal(30, result.DurationMinutes);
    }

    [Fact]
    public async Task GetByCourseIdAsync_ShouldReturnLessons_WhenCourseExists()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        var courseContent = new CourseContent(
            "Syllabus", 
            "Learning Objectives", 
            "Prerequisites", 
            "Target Audience", 
            "Portuguese", 
            "Beginner"
        );
        
        var course = new Course("Test Course", "Test Description", courseContent, 100m)
        {
            Name = "Test Course",
            Description = "Test Description",
            Content = courseContent
        };
        
        course.AddLesson("Lesson 1", "Content 1", "Description 1", 1, 30);
        course.AddLesson("Lesson 2", "Content 2", "Description 2", 2, 45);

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.GetByCourseIdAsync(course.Id);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task AddAsync_ShouldAddLesson_WhenValidLesson()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        var courseContent = new CourseContent(
            "Syllabus", 
            "Learning Objectives", 
            "Prerequisites", 
            "Target Audience", 
            "Portuguese", 
            "Beginner"
        );
        
        var course = new Course("Test Course", "Test Description", courseContent, 100m)
        {
            Name = "Test Course",
            Description = "Test Description",
            Content = courseContent
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var lesson = new Lesson("New Lesson", "New Content", "New Description", course, 1, 60);

        // Act
        await repository.AddAsync(lesson);
        await context.SaveChangesAsync();

        // Assert
        var savedLesson = await context.Lessons.FirstOrDefaultAsync(l => l.Id == lesson.Id);
        Assert.NotNull(savedLesson);
        Assert.Equal("New Lesson", savedLesson.Title);
        Assert.Equal(60, savedLesson.DurationMinutes);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenLessonNotExists()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCourseIdAsync_ShouldReturnEmpty_WhenCourseHasNoLessons()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        var courseContent = new CourseContent(
            "Syllabus", 
            "Learning Objectives", 
            "Prerequisites", 
            "Target Audience", 
            "Portuguese", 
            "Beginner"
        );
        
        var course = new Course("Test Course", "Test Description", courseContent, 100m)
        {
            Name = "Test Course",
            Description = "Test Description",
            Content = courseContent
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.GetByCourseIdAsync(course.Id);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetByCourseIdAsync_ShouldReturnOrderedLessons_WhenCourseExists()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        var courseContent = new CourseContent(
            "Syllabus", 
            "Learning Objectives", 
            "Prerequisites", 
            "Target Audience", 
            "Portuguese", 
            "Beginner"
        );
        
        var course = new Course("Test Course", "Test Description", courseContent, 100m)
        {
            Name = "Test Course",
            Description = "Test Description",
            Content = courseContent
        };

        course.AddLesson("Lesson 1", "Content 1", "Description 1", 3, 30);
        course.AddLesson("Lesson 2", "Content 2", "Description 2", 1, 45);
        course.AddLesson("Lesson 3", "Content 3", "Description 3", 2, 60);

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.GetByCourseIdAsync(course.Id);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(3, results.Count());
        
        var orderedResults = results.OrderBy(l => l.Order).ToList();
        Assert.Equal(1, orderedResults[0].Order);
        Assert.Equal(2, orderedResults[1].Order);
        Assert.Equal(3, orderedResults[2].Order);
    }

    [Fact]
    public async Task GetByCourseIdAsync_ShouldReturnEmpty_WhenCourseNotExists()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        // Act
        var results = await repository.GetByCourseIdAsync(Guid.NewGuid());

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateLesson_WhenValidLesson()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        var courseContent = new CourseContent(
            "Syllabus", 
            "Learning Objectives", 
            "Prerequisites", 
            "Target Audience", 
            "Portuguese", 
            "Beginner"
        );
        
        var course = new Course("Test Course", "Test Description", courseContent, 100m)
        {
            Name = "Test Course",
            Description = "Test Description",
            Content = courseContent
        };

        course.AddLesson("Original Title", "Original Content", "Original Description", 1, 30);

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var lesson = course.Lessons.First();

        // Act
        lesson.Update("Updated Title", "Updated Description", "Updated Content", null, 45);
        await repository.UpdateAsync(lesson);
        await context.SaveChangesAsync();

        // Assert
        var updatedLesson = await context.Lessons.FirstOrDefaultAsync(l => l.Id == lesson.Id);
        Assert.NotNull(updatedLesson);
        Assert.Equal("Updated Title", updatedLesson.Title);
        Assert.Equal(45, updatedLesson.DurationMinutes);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteLesson_WhenLessonExists()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        var courseContent = new CourseContent(
            "Syllabus", 
            "Learning Objectives", 
            "Prerequisites", 
            "Target Audience", 
            "Portuguese", 
            "Beginner"
        );
        
        var course = new Course("Test Course", "Test Description", courseContent, 100m)
        {
            Name = "Test Course",
            Description = "Test Description",
            Content = courseContent
        };

        course.AddLesson("Test Lesson", "Test Content", "Test Description", 1, 30);

        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var lesson = course.Lessons.First();

        // Act
        await repository.DeleteAsync(lesson.Id);

        // Assert
        var deletedLesson = await context.Lessons.FirstOrDefaultAsync(l => l.Id == lesson.Id);
        Assert.Null(deletedLesson);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenLessonNotExists()
    {
        // Arrange
        using var context = new ContentDbContext(_options);
        var repository = new LessonRepository(context, _mockEventService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<FluencyHub.SharedKernel.Common.Exceptions.NotFoundException>(() => repository.DeleteAsync(Guid.NewGuid()));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 