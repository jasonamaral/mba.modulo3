using FluencyHub.ContentManagement.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace FluencyHub.Tests.Domain.ContentManagement;

public class CourseTests
{
    private readonly CourseContent _validContent;
    
    public CourseTests()
    {
        _validContent = new CourseContent(
            "Complete Syllabus",
            "Learning Objectives",
            "No Prerequisites",
            "Beginners",
            "English",
            "A1");
    }
    
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var course = new Course("Test Course", "Course description", _validContent, 99.99m);
        
        // Assert
        Assert.Equal("Test Course", course.Name);
        Assert.Equal("Course description", course.Description);
        Assert.Equal(_validContent, course.Content);
        Assert.Equal(99.99m, course.Price);
        Assert.True(course.IsActive);
        Assert.Equal(CourseStatus.Draft, course.Status);
        Assert.Null(course.PublishedAt);
        Assert.Empty(course.Lessons);
    }
    
    [Theory]
    [InlineData("", "Description", "Course name cannot be empty")]
    [InlineData("  ", "Description", "Course name cannot be empty")]
    [InlineData("Name", "", "Description cannot be empty")]
    [InlineData("Name", "  ", "Description cannot be empty")]
    public void Constructor_WithInvalidParameters_ThrowsArgumentException(string name, string description, string expectedErrorMessage)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Course(name, description, _validContent, 99.99m));
        Assert.Contains(expectedErrorMessage, exception.Message);
    }
    
    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Course("Course", "Description", _validContent, -10.0m));
        Assert.Contains("Price cannot be negative", exception.Message);
    }
    
    [Fact]
    public void Constructor_WithNullContent_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Course("Course", "Description", null, 99.99m));
        Assert.Contains("Course content cannot be null", exception.Message);
    }
    
    [Fact]
    public void UpdateDetails_WithValidParameters_UpdatesCourse()
    {
        // Arrange
        var course = new Course("Old Course", "Old description", _validContent, 99.99m);
        var newContent = new CourseContent(
            "Updated Syllabus",
            "Updated Objectives",
            "Updated Prerequisites",
            "Advanced",
            "Spanish",
            "B2");
        
        // Act
        course.UpdateDetails("New Course", "New description", newContent, 149.99m);
        
        // Assert
        Assert.Equal("New Course", course.Name);
        Assert.Equal("New description", course.Description);
        Assert.Equal(newContent, course.Content);
        Assert.Equal(149.99m, course.Price);
        Assert.NotNull(course.UpdatedAt);
    }
    
    [Fact]
    public void AddLesson_ToActiveCourse_AddsLesson()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        
        // Act
        var lesson = course.AddLesson("Lesson 1", "Lesson content", "http://materials.com/lesson1");
        
        // Assert
        Assert.Single(course.Lessons);
        Assert.Equal("Lesson 1", lesson.Title);
        Assert.Equal("Lesson content", lesson.Content);
        Assert.Equal("http://materials.com/lesson1", lesson.MaterialUrl);
        Assert.Equal(1, lesson.Order);
        Assert.Equal(course.Id, lesson.CourseId);
    }
    
    [Fact]
    public void AddLesson_ToInactiveCourse_ThrowsException()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        course.Deactivate();
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            course.AddLesson("Lesson 1", "Lesson content", "http://materials.com/lesson1"));
        Assert.Contains("Cannot add lessons to an inactive course", exception.Message);
    }
    
    [Fact]
    public void RemoveLesson_ExistingLesson_RemovesAndReordersLessons()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        
        // Definindo um ID para o curso
        var courseId = Guid.NewGuid();
        typeof(Course).GetProperty("Id").SetValue(course, courseId);
        
        // Criando lições com IDs definidos
        var lesson1 = course.AddLesson("Lesson 1", "Content 1", "http://materials.com/lesson1");
        var lesson2 = course.AddLesson("Lesson 2", "Content 2", "http://materials.com/lesson2");
        var lesson3 = course.AddLesson("Lesson 3", "Content 3", "http://materials.com/lesson3");
        
        // Definir IDs usando reflection
        var lesson1Id = Guid.NewGuid();
        var lesson2Id = Guid.NewGuid();
        var lesson3Id = Guid.NewGuid();
        
        typeof(Lesson).GetProperty("Id").SetValue(lesson1, lesson1Id);
        typeof(Lesson).GetProperty("Id").SetValue(lesson2, lesson2Id);
        typeof(Lesson).GetProperty("Id").SetValue(lesson3, lesson3Id);
        
        // Act
        course.RemoveLesson(lesson2Id);
        
        // Assert
        Assert.Equal(2, course.Lessons.Count);
        Assert.DoesNotContain(course.Lessons, l => l.Id == lesson2Id);
        
        var remainingLessons = course.Lessons.OrderBy(l => l.Order).ToList();
        Assert.Equal(1, remainingLessons[0].Order);
        Assert.Equal(2, remainingLessons[1].Order);
    }
    
    [Fact]
    public void PublishCourse_ChangesStatusAndSetsPublishedAt()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        
        // Act
        course.PublishCourse();
        
        // Assert
        Assert.Equal(CourseStatus.Published, course.Status);
        Assert.NotNull(course.PublishedAt);
        Assert.True(course.PublishedAt <= DateTime.UtcNow);
    }
    
    [Fact]
    public void ArchiveCourse_ChangesStatusAndDeactivatesCourse()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        
        // Act
        course.ArchiveCourse();
        
        // Assert
        Assert.Equal(CourseStatus.Archived, course.Status);
        Assert.False(course.IsActive);
    }
    
    [Fact]
    public void ReorderLesson_MovesLessonAndUpdatesOtherLessonsOrder()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        
        // Adicionar lições através do método público AddLesson
        var lesson1 = course.AddLesson("Lesson 1", "Content 1", null);
        var lesson2 = course.AddLesson("Lesson 2", "Content 2", null);
        var lesson3 = course.AddLesson("Lesson 3", "Content 3", null);
        
        // Definir IDs usando reflection
        var lesson1Id = Guid.NewGuid();
        var lesson2Id = Guid.NewGuid();
        var lesson3Id = Guid.NewGuid();
        
        typeof(Lesson).GetProperty("Id").SetValue(lesson1, lesson1Id);
        typeof(Lesson).GetProperty("Id").SetValue(lesson2, lesson2Id);
        typeof(Lesson).GetProperty("Id").SetValue(lesson3, lesson3Id);
        
        // Act
        course.ReorderLesson(lesson3Id, 1);
        
        // Assert
        var orderedLessons = course.Lessons.OrderBy(l => l.Order).ToList();
        Assert.Equal(lesson3Id, orderedLessons[0].Id);
        Assert.Equal(lesson1Id, orderedLessons[1].Id);
        Assert.Equal(lesson2Id, orderedLessons[2].Id);
        
        Assert.Equal(1, orderedLessons[0].Order);
        Assert.Equal(2, orderedLessons[1].Order);
        Assert.Equal(3, orderedLessons[2].Order);
    }
    
    [Fact]
    public void Activate_InactiveCourse_SetsIsActiveToTrue()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        course.Deactivate();
        Assert.False(course.IsActive);
        
        // Act
        course.Activate();
        
        // Assert
        Assert.True(course.IsActive);
    }
    
    [Fact]
    public void Deactivate_ActiveCourse_SetsIsActiveToFalse()
    {
        // Arrange
        var course = new Course("Course", "Description", _validContent, 99.99m);
        Assert.True(course.IsActive);
        
        // Act
        course.Deactivate();
        
        // Assert
        Assert.False(course.IsActive);
    }
} 