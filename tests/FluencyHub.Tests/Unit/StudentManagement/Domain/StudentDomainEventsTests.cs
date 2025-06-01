using FluencyHub.StudentManagement.Domain.Events;
using Xunit;

namespace FluencyHub.Tests.Unit.StudentManagement.Domain;

public class StudentDomainEventsTests
{
    [Fact]
    public void StudentDeactivatedEvent_Constructor_ShouldCreateEvent_WhenValidStudentId()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Act
        var studentEvent = new StudentDeactivatedEvent(studentId);

        // Assert
        Assert.Equal(studentId, studentEvent.StudentId);
        Assert.NotEqual(Guid.Empty, studentEvent.EventId);
        Assert.True(studentEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(studentEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void StudentActivatedEvent_Constructor_ShouldCreateEvent_WhenValidStudentId()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Act
        var studentEvent = new StudentActivatedEvent(studentId);

        // Assert
        Assert.Equal(studentId, studentEvent.StudentId);
        Assert.NotEqual(Guid.Empty, studentEvent.EventId);
        Assert.True(studentEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(studentEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void StudentEnrolledEvent_Constructor_ShouldCreateEvent_WhenValidParameters()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enrollmentId = Guid.NewGuid();

        // Act
        var studentEvent = new StudentEnrolledEvent(studentId, courseId, enrollmentId);

        // Assert
        Assert.Equal(studentId, studentEvent.StudentId);
        Assert.Equal(courseId, studentEvent.CourseId);
        Assert.Equal(enrollmentId, studentEvent.EnrollmentId);
        Assert.NotEqual(Guid.Empty, studentEvent.EventId);
        Assert.True(studentEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(studentEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void CertificateIssuedEvent_Constructor_ShouldCreateEvent_WhenValidParameters()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var certificateId = Guid.NewGuid();

        // Act
        var certificateEvent = new CertificateIssuedEvent(studentId, courseId, certificateId);

        // Assert
        Assert.Equal(studentId, certificateEvent.StudentId);
        Assert.Equal(courseId, certificateEvent.CourseId);
        Assert.Equal(certificateId, certificateEvent.CertificateId);
        Assert.NotEqual(Guid.Empty, certificateEvent.EventId);
        Assert.True(certificateEvent.OccurredOn <= DateTime.UtcNow);
        Assert.True(certificateEvent.OccurredOn >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void StudentEvents_ShouldHaveUniqueEventIds()
    {
        // Arrange & Act
        var event1 = new StudentActivatedEvent(Guid.NewGuid());
        var event2 = new StudentDeactivatedEvent(Guid.NewGuid());
        var event3 = new StudentEnrolledEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var event4 = new CertificateIssuedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.NotEqual(event1.EventId, event2.EventId);
        Assert.NotEqual(event1.EventId, event3.EventId);
        Assert.NotEqual(event1.EventId, event4.EventId);
        Assert.NotEqual(event2.EventId, event3.EventId);
        Assert.NotEqual(event2.EventId, event4.EventId);
        Assert.NotEqual(event3.EventId, event4.EventId);
    }

    [Fact]
    public void StudentEvents_ShouldHaveValidOccurredOnTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var activatedEvent = new StudentActivatedEvent(Guid.NewGuid());
        var deactivatedEvent = new StudentDeactivatedEvent(Guid.NewGuid());
        var enrolledEvent = new StudentEnrolledEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var certificateEvent = new CertificateIssuedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(activatedEvent.OccurredOn >= beforeCreation && activatedEvent.OccurredOn <= afterCreation);
        Assert.True(deactivatedEvent.OccurredOn >= beforeCreation && deactivatedEvent.OccurredOn <= afterCreation);
        Assert.True(enrolledEvent.OccurredOn >= beforeCreation && enrolledEvent.OccurredOn <= afterCreation);
        Assert.True(certificateEvent.OccurredOn >= beforeCreation && certificateEvent.OccurredOn <= afterCreation);
    }

    [Fact]
    public void StudentActivatedEvent_ShouldAcceptEmptyGuid()
    {
        // Arrange & Act
        var studentEvent = new StudentActivatedEvent(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, studentEvent.StudentId);
        Assert.NotEqual(Guid.Empty, studentEvent.EventId);
    }

    [Fact]
    public void StudentDeactivatedEvent_ShouldAcceptEmptyGuid()
    {
        // Arrange & Act
        var studentEvent = new StudentDeactivatedEvent(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, studentEvent.StudentId);
        Assert.NotEqual(Guid.Empty, studentEvent.EventId);
    }

    [Fact]
    public void StudentEnrolledEvent_ShouldAcceptEmptyGuids()
    {
        // Arrange & Act
        var studentEvent = new StudentEnrolledEvent(Guid.Empty, Guid.Empty, Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, studentEvent.StudentId);
        Assert.Equal(Guid.Empty, studentEvent.CourseId);
        Assert.Equal(Guid.Empty, studentEvent.EnrollmentId);
        Assert.NotEqual(Guid.Empty, studentEvent.EventId);
    }

    [Fact]
    public void CertificateIssuedEvent_ShouldAcceptEmptyGuids()
    {
        // Arrange & Act
        var certificateEvent = new CertificateIssuedEvent(Guid.Empty, Guid.Empty, Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, certificateEvent.StudentId);
        Assert.Equal(Guid.Empty, certificateEvent.CourseId);
        Assert.Equal(Guid.Empty, certificateEvent.CertificateId);
        Assert.NotEqual(Guid.Empty, certificateEvent.EventId);
    }
} 