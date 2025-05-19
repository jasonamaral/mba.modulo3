using FluencyHub.ContentManagement.Domain;
using FluencyHub.Infrastructure.Persistence;
using FluencyHub.Infrastructure.Persistence.Extensions;
using FluencyHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace FluencyHub.Tests.Infrastructure.Repositories;

public class CourseRepositoryTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<FluencyHubDbContext> _contextOptions;
    private readonly FluencyHubDbContext _context;
    private readonly CourseRepository _repository;

    public CourseRepositoryTests()
    {
        // Create and open a connection to the SQLite in-memory database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Configure the context to use the SQLite connection
        _contextOptions = new DbContextOptionsBuilder<FluencyHubDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the schema in the database
        _context = CreateContext();
        _context.Database.EnsureCreated();

        _repository = new CourseRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private FluencyHubDbContext CreateContext()
    {
        return new FluencyHubDbContext(_contextOptions);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCourse_ShouldReturnCourse()
    {
        // Arrange
        var course = CreateTestCourse();
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        // Use a new context instance to ensure data is fetched from the database
        using var context = CreateContext();
        var repository = new CourseRepository(context);

        // Act
        var result = await repository.GetByIdAsync(course.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(course.Id, result.Id);
        Assert.Equal(course.Name, result.Name);
        Assert.Equal(course.Description, result.Description);
        Assert.Equal(course.Price, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCourse_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCourses()
    {
        // Arrange
        var course1 = CreateTestCourse("English for Beginners");
        var course2 = CreateTestCourse("Spanish for Intermediates");
        _context.Courses.AddRange(course1, course2);
        await _context.SaveChangesAsync();

        // Use a new context instance to ensure data is fetched from the database
        using var context = CreateContext();
        var repository = new CourseRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Id == course1.Id && c.Name == course1.Name);
        Assert.Contains(result, c => c.Id == course2.Id && c.Name == course2.Name);
    }

    [Fact]
    public async Task GetActiveCoursesAsync_ShouldReturnOnlyActiveCourses()
    {
        // Arrange
        var activeCourse = CreateTestCourse("Active Course");
        var inactiveCourse = CreateTestCourse("Inactive Course");
        inactiveCourse.Deactivate();
        
        _context.Courses.AddRange(activeCourse, inactiveCourse);
        await _context.SaveChangesAsync();

        // Use a new context instance to ensure data is fetched from the database
        using var context = CreateContext();
        var repository = new CourseRepository(context);

        // Act
        var result = await repository.GetActiveCoursesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(activeCourse.Id, result.First().Id);
        Assert.Equal(activeCourse.Name, result.First().Name);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCourseToDatabase()
    {
        // Arrange
        var course = CreateTestCourse();

        // Act
        await _repository.AddAsync(course);
        await _repository.SaveChangesAsync();

        // Assert
        using var context = CreateContext();
        var savedCourse = await context.Courses.FirstOrDefaultAsync(c => c.Id == course.Id);
        
        Assert.NotNull(savedCourse);
        Assert.Equal(course.Name, savedCourse.Name);
        Assert.Equal(course.Description, savedCourse.Description);
        Assert.Equal(course.Price, savedCourse.Price);
    }

    [Fact]
    public async Task GetByLanguageAsync_ShouldReturnCoursesWithSpecificLanguage()
    {
        // Arrange
        var englishCourse = CreateTestCourse("English Course", "English");
        var spanishCourse = CreateTestCourse("Spanish Course", "Spanish");
        var frenchCourse = CreateTestCourse("French Course", "French");
        
        _context.Courses.AddRange(englishCourse, spanishCourse, frenchCourse);
        await _context.SaveChangesAsync();

        // Use a new context instance to ensure data is fetched from the database
        using var context = CreateContext();
        var repository = new CourseRepository(context);

        // Act
        var result = await repository.GetByLanguageAsync("Spanish");

        // Assert
        Assert.Single(result);
        Assert.Equal(spanishCourse.Id, result.First().Id);
        Assert.Equal(spanishCourse.Name, result.First().Name);
        Assert.Equal("Spanish", result.First().Content.Language);
    }

    [Fact]
    public async Task GetByLevelAsync_ShouldReturnCoursesWithSpecificLevel()
    {
        // Arrange
        var beginnerCourse = CreateTestCourse("Beginner Course", "English", "A1");
        var intermediateCourse = CreateTestCourse("Intermediate Course", "English", "B1");
        var advancedCourse = CreateTestCourse("Advanced Course", "English", "C1");
        
        _context.Courses.AddRange(beginnerCourse, intermediateCourse, advancedCourse);
        await _context.SaveChangesAsync();

        // Use a new context instance to ensure data is fetched from the database
        using var context = CreateContext();
        var repository = new CourseRepository(context);

        // Act
        var result = await repository.GetByLevelAsync("B1");

        // Assert
        Assert.Single(result);
        Assert.Equal(intermediateCourse.Id, result.First().Id);
        Assert.Equal(intermediateCourse.Name, result.First().Name);
        Assert.Equal("B1", result.First().Content.Level);
    }

    private Course CreateTestCourse(string name = "Test Course", string language = "English", string level = "A1")
    {
        var content = new CourseContent(
            "Test Syllabus",
            "Test Learning Objectives",
            "Test Prerequisites",
            "Test Target Audience",
            language,
            level
        );
        
        return new Course(
            name,
            "Test Description",
            content,
            99.99m
        );
    }
} 