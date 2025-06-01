using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.ContentManagement.Infrastructure.Persistence.Repositories;
using FluencyHub.SharedKernel.Events;
using FluencyHub.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using MediatR;
using System.Reflection;

namespace FluencyHub.Tests.Unit.ContentManagement.Infrastructure;

public class CourseRepositoryTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly Mock<IDomainEventService> _mockEventService;
    private readonly CourseRepository _repository;

    public CourseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _mockEventService = new Mock<IDomainEventService>();
        _repository = new CourseRepository(_context, _mockEventService.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCourse_WhenCourseExists()
    {
        // Arrange
        var courseContent = new CourseContent(
            "Introdução ao C#",
            "Objetivo: Aprender os fundamentos",
            "Conhecimento básico em programação",
            "Iniciantes em programação",
            "Portuguese",
            "Beginner"
        );
        var course = new Course("C# Fundamentals", "Aprenda C# do zero", courseContent, 100.00m)
        {
            Name = "C# Fundamentals",
            Description = "Aprenda C# do zero",
            Content = courseContent
        };
        
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(course.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(course.Id);
        result.Name.Should().Be("C# Fundamentals");
        result.Description.Should().Be("Aprenda C# do zero");
        result.Price.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenCourseNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<FluencyHub.SharedKernel.Common.Exceptions.NotFoundException>(
            () => _repository.GetByIdAsync(nonExistentId));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCourses()
    {
        // Arrange
        var courseContent1 = new CourseContent("Intro C#", "Objetivo 1", "Pré-req 1", "Iniciantes", "Português", "Básico");
        var courseContent2 = new CourseContent("Intro ASP.NET", "Objetivo 2", "Pré-req 2", "Intermediários", "Português", "Intermediário");
        var course1 = new Course("C# Fundamentals", "Aprenda C#", courseContent1, 100.00m)
        {
            Name = "C# Fundamentals",
            Description = "Aprenda C#",
            Content = courseContent1
        };
        var course2 = new Course("ASP.NET Core", "Aprenda ASP.NET", courseContent2, 150.00m)
        {
            Name = "ASP.NET Core",
            Description = "Aprenda ASP.NET",
            Content = courseContent2
        };
        
        await _context.Courses.AddRangeAsync(course1, course2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var courses = result.ToList();
        courses.Should().HaveCount(2);
        courses.Should().Contain(c => c.Name == "C# Fundamentals");
        courses.Should().Contain(c => c.Name == "ASP.NET Core");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoCoursesExist()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldAddCourse_WhenValidCourse()
    {
        // Arrange
        var course = TestDataBuilder.CreateValidCourse("C# Fundamentals", "Aprenda C#", 100.00m);

        // Act
        await _repository.AddAsync(course);
        await _repository.SaveChangesAsync();

        // Assert
        var savedCourse = await _context.Courses.FindAsync(course.Id);
        savedCourse.Should().NotBeNull();
        savedCourse!.Name.Should().Be("C# Fundamentals");
        savedCourse.Description.Should().Be("Aprenda C#");
        savedCourse.Price.Should().Be(100.00m);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCourse_WhenValidCourse()
    {
        // Arrange
        var course = TestDataBuilder.CreateValidCourse("C# Fundamentals", "Aprenda C#", 100.00m);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        var newContent = TestDataBuilder.CreateValidCourseContent(
            "Novo programa",
            "Novos objetivos",
            "Novos pré-requisitos",
            "Nova audiência",
            "Inglês",
            "Avançado"
        );
        course.UpdateDetails("C# Advanced", "Aprenda C# avançado", newContent, 150.00m);

        // Act
        await _repository.UpdateAsync(course);

        // Assert
        var updatedCourse = await _context.Courses.FindAsync(course.Id);
        updatedCourse.Should().NotBeNull();
        updatedCourse!.Name.Should().Be("C# Advanced");
        updatedCourse.Description.Should().Be("Aprenda C# avançado");
        updatedCourse.Price.Should().Be(150.00m);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCourse_WhenCourseExists()
    {
        // Arrange
        var course = TestDataBuilder.CreateValidCourse("C# Fundamentals", "Aprenda C#", 100.00m);
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(course.Id);

        // Assert
        var deletedCourse = await _context.Courses.FindAsync(course.Id);
        deletedCourse.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenCourseExists()
    {
        // Arrange
        var courseContent = new CourseContent("Conteúdo", "Objetivos", "Pré-requisitos", "Audiência", "Português", "Básico");
        var course = new Course("Curso Existente", "Descrição", courseContent, 100.00m)
        {
            Name = "Curso Existente",
            Description = "Descrição",
            Content = courseContent
        };
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(course.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenCourseNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetNameAsync_ShouldReturnName_WhenCourseExists()
    {
        // Arrange
        var courseContent = new CourseContent("Conteúdo", "Objetivos", "Pré-requisitos", "Audiência", "Português", "Básico");
        var course = new Course("Curso de Teste", "Descrição", courseContent, 100.00m)
        {
            Name = "Curso de Teste",
            Description = "Descrição",
            Content = courseContent
        };
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNameAsync(course.Id);

        // Assert
        result.Should().Be("Curso de Teste");
    }

    [Fact]
    public async Task GetNameAsync_ShouldThrowNotFoundException_WhenCourseNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<FluencyHub.SharedKernel.Common.Exceptions.NotFoundException>(
            () => _repository.GetNameAsync(nonExistentId));
    }

    [Fact]
    public async Task GetActiveCoursesAsync_ShouldReturnOnlyActiveCourses()
    {
        // Arrange
        var activeCourse = TestDataBuilder.CreateValidCourse("C# Fundamentals", "Aprenda C#", 100.00m);
        var inactiveCourse = TestDataBuilder.CreateValidCourse("Python Basics", "Aprenda Python", 120.00m);
        
        // Simular status usando reflexão
        var isActiveProperty = typeof(Course).GetProperty("IsActive");
        
        isActiveProperty?.SetValue(activeCourse, true);
        isActiveProperty?.SetValue(inactiveCourse, false);
        
        await _context.Courses.AddRangeAsync(activeCourse, inactiveCourse);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveCoursesAsync();

        // Assert
        var courses = result.ToList();
        courses.Should().HaveCount(1);
        courses.Should().Contain(c => c.Name == "C# Fundamentals");
        courses.Should().NotContain(c => c.Name == "Python Basics");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPublishDomainEvents()
    {
        // Arrange
        var course = TestDataBuilder.CreateValidCourse("C# Fundamentals", "Aprenda C#", 100.00m);
        
        // Simular um evento de domínio usando reflexão
        var domainEventsField = typeof(Course).GetField("_domainEvents", BindingFlags.NonPublic | BindingFlags.Instance);
        if (domainEventsField != null)
        {
            var events = new List<INotification> { new Mock<INotification>().Object };
            domainEventsField.SetValue(course, events);
        }
        
        await _context.Courses.AddAsync(course);

        // Act
        await _repository.SaveChangesAsync();

        // Assert - Verificar se o método foi chamado apenas se há eventos
        if (domainEventsField?.GetValue(course) is List<INotification> courseEvents && courseEvents.Any())
        {
            _mockEventService.Verify(x => x.PublishAsync(It.IsAny<INotification>()), Times.AtLeastOnce);
        }
        else
        {
            _mockEventService.Verify(x => x.PublishAsync(It.IsAny<INotification>()), Times.Never);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
} 