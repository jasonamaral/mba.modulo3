using FluencyHub.ContentManagement.Domain;
using FluencyHub.ContentManagement.Domain.Events;
using Xunit;

namespace FluencyHub.Tests.Unit.ContentManagement.Domain;

public class CourseDomainEventsTests
{
    private Course CreateValidCourse()
    {
        var content = new CourseContent(
            "Syllabus completo",
            "Objetivos de aprendizado",
            "Pré-requisitos",
            "Público alvo",
            "Português",
            "Iniciante"
        );
        var name = "Curso de Teste";
        var description = "Descrição do curso";
        return new Course(name, description, content, 100m)
        {
            Name = name,
            Description = description,
            Content = content
        };
    }

    [Fact]
    public void CourseCreatedDomainEvent_Constructor_ShouldCreateEvent_WhenValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CourseCreatedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.Equal(course.Name, courseEvent.Name);
        Assert.Equal(course.Description, courseEvent.Description);
        Assert.Equal(course.Price, courseEvent.Price);
        Assert.Equal(course.IsActive, courseEvent.IsActive);
        Assert.Equal(course.Status, courseEvent.Status);
        Assert.Equal(course.Content, courseEvent.Content);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
        Assert.True(courseEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(courseEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CourseUpdatedDomainEvent_Constructor_ShouldCreateEvent_WhenValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CourseUpdatedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.Equal(course.Name, courseEvent.Name);
        Assert.Equal(course.Description, courseEvent.Description);
        Assert.Equal(course.Price, courseEvent.Price);
        Assert.Equal(course.IsActive, courseEvent.IsActive);
        Assert.Equal(course.Status, courseEvent.Status);
        Assert.Equal(course.Content, courseEvent.Content);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
        Assert.True(courseEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(courseEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CourseActivatedDomainEvent_Constructor_ShouldCreateEvent_WhenValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CourseActivatedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
        Assert.True(courseEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(courseEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CourseDeactivatedDomainEvent_Constructor_ShouldCreateEvent_WhenValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CourseDeactivatedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
        Assert.True(courseEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(courseEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CoursePublishedDomainEvent_Constructor_ShouldCreateEvent_WhenValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CoursePublishedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.Equal(course.Name, courseEvent.Name);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
        Assert.True(courseEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(courseEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CourseArchivedDomainEvent_Constructor_ShouldCreateEvent_WhenValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CourseArchivedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
        Assert.True(courseEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(courseEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CourseDeletedDomainEvent_Constructor_ShouldCreateEvent_WhenValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CourseDeletedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
        Assert.True(courseEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(courseEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CourseEvents_ShouldHaveUniqueEventIds()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var event1 = new CourseCreatedDomainEvent(course);
        var event2 = new CourseUpdatedDomainEvent(course);
        var event3 = new CourseActivatedDomainEvent(course);
        var event4 = new CourseDeactivatedDomainEvent(course);
        var event5 = new CoursePublishedDomainEvent(course);
        var event6 = new CourseArchivedDomainEvent(course);
        var event7 = new CourseDeletedDomainEvent(course);

        // Assert
        var eventIds = new[] { event1.EventId, event2.EventId, event3.EventId, event4.EventId, event5.EventId, event6.EventId, event7.EventId };
        Assert.Equal(eventIds.Length, eventIds.Distinct().Count());
    }

    [Fact]
    public void CourseEvents_ShouldHaveValidOccurredOnTime()
    {
        // Arrange
        var course = CreateValidCourse();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var createdEvent = new CourseCreatedDomainEvent(course);
        var updatedEvent = new CourseUpdatedDomainEvent(course);
        var activatedEvent = new CourseActivatedDomainEvent(course);
        var deactivatedEvent = new CourseDeactivatedDomainEvent(course);
        var publishedEvent = new CoursePublishedDomainEvent(course);
        var archivedEvent = new CourseArchivedDomainEvent(course);
        var deletedEvent = new CourseDeletedDomainEvent(course);

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(createdEvent.OccurredOn >= beforeCreation && createdEvent.OccurredOn <= afterCreation);
        Assert.True(updatedEvent.OccurredOn >= beforeCreation && updatedEvent.OccurredOn <= afterCreation);
        Assert.True(activatedEvent.OccurredOn >= beforeCreation && activatedEvent.OccurredOn <= afterCreation);
        Assert.True(deactivatedEvent.OccurredOn >= beforeCreation && deactivatedEvent.OccurredOn <= afterCreation);
        Assert.True(publishedEvent.OccurredOn >= beforeCreation && publishedEvent.OccurredOn <= afterCreation);
        Assert.True(archivedEvent.OccurredOn >= beforeCreation && archivedEvent.OccurredOn <= afterCreation);
        Assert.True(deletedEvent.OccurredOn >= beforeCreation && deletedEvent.OccurredOn <= afterCreation);
    }

    [Fact]
    public void CourseCreatedDomainEvent_ShouldAcceptNullCourse()
    {
        // Arrange
        Course course = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new CourseCreatedDomainEvent(course));
    }

    [Fact]
    public void CourseDeletedDomainEvent_ShouldAcceptValidCourse()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var courseEvent = new CourseDeletedDomainEvent(course);

        // Assert
        Assert.Equal(course.Id, courseEvent.CourseId);
        Assert.NotEqual(Guid.Empty, courseEvent.EventId);
    }
} 