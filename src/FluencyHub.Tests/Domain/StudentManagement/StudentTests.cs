using FluencyHub.StudentManagement.Domain;
using System;
using Xunit;

namespace FluencyHub.Tests.Domain.StudentManagement;

public class StudentTests
{
    [Fact]
    public void Create_Student_WithValidData_ShouldSucceed()
    {
        // Arrange
        string firstName = "John";
        string lastName = "Doe";
        string email = "john.doe@example.com";
        DateTime dateOfBirth = new DateTime(1990, 1, 1);
        string phoneNumber = "123-456-7890";

        // Act
        var student = new Student(firstName, lastName, email, dateOfBirth, phoneNumber);

        // Assert
        Assert.Equal(firstName, student.FirstName);
        Assert.Equal(lastName, student.LastName);
        Assert.Equal(email, student.Email);
        Assert.Equal(dateOfBirth, student.DateOfBirth);
        Assert.Equal(phoneNumber, student.PhoneNumber);
        Assert.True(student.IsActive);
        Assert.Empty(student.Enrollments);
        Assert.Empty(student.Certificates);
        Assert.Equal($"{firstName} {lastName}", student.FullName);
    }

    [Theory]
    [InlineData("", "Doe", "john.doe@example.com")]
    [InlineData("John", "", "john.doe@example.com")]
    [InlineData("John", "Doe", "")]
    public void Create_Student_WithInvalidData_ShouldThrowException(string firstName, string lastName, string email)
    {
        // Arrange
        DateTime dateOfBirth = new DateTime(1990, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Student(firstName, lastName, email, dateOfBirth));
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateStudentProperties()
    {
        // Arrange
        var student = CreateValidStudent();
        string newFirstName = "Jane";
        string newLastName = "Smith";
        DateTime newDateOfBirth = new DateTime(1992, 2, 2);
        string newPhoneNumber = "987-654-3210";

        // Act
        student.UpdateDetails(newFirstName, newLastName, newDateOfBirth, newPhoneNumber);

        // Assert
        Assert.Equal(newFirstName, student.FirstName);
        Assert.Equal(newLastName, student.LastName);
        Assert.Equal(newDateOfBirth, student.DateOfBirth);
        Assert.Equal(newPhoneNumber, student.PhoneNumber);
        Assert.Equal($"{newFirstName} {newLastName}", student.FullName);
    }

    [Theory]
    [InlineData("", "Smith")]
    [InlineData("Jane", "")]
    public void UpdateDetails_WithInvalidParameters_ShouldThrowArgumentException(
        string firstName,
        string lastName)
    {
        // Arrange
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => student.UpdateDetails(
            firstName,
            lastName,
            new DateTime(1992, 2, 2),
            "987654321"));
    }

    [Fact]
    public void EnrollInCourse_ActiveStudent_ShouldCreateEnrollment()
    {
        // Arrange
        var student = CreateValidStudent();
        Guid courseId = Guid.NewGuid();
        decimal price = 99.99m;

        // Act
        var enrollment = student.EnrollInCourse(courseId, price);

        // Assert
        Assert.Single(student.Enrollments);
        Assert.Equal(courseId, enrollment.CourseId);
        Assert.Equal(student.Id, enrollment.StudentId);
        Assert.Equal(price, enrollment.Price);
        Assert.Equal(StatusMatricula.AguardandoPagamento, enrollment.Status);
    }

    [Fact]
    public void EnrollInCourse_InactiveStudent_ShouldThrowException()
    {
        // Arrange
        var student = CreateValidStudent();
        student.Deactivate();
        Guid courseId = Guid.NewGuid();
        decimal price = 99.99m;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            student.EnrollInCourse(courseId, price));
    }

    [Fact]
    public void EnrollInCourse_AlreadyEnrolled_ShouldThrowException()
    {
        // Arrange
        var student = CreateValidStudent();
        Guid courseId = Guid.NewGuid();
        student.EnrollInCourse(courseId, 99.99m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            student.EnrollInCourse(courseId, 99.99m));
    }

    [Fact]
    public void AddCertificate_ForCompletedCourse_ShouldAddCertificate()
    {
        // Arrange
        var student = CreateValidStudent();
        Guid courseId = Guid.NewGuid();
        var enrollment = student.EnrollInCourse(courseId, 99.99m);
        
        // Need to activate the enrollment first, then complete it
        typeof(Enrollment).GetMethod("ActivateEnrollment", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Invoke(enrollment, null);
            
        // Now complete the enrollment
        typeof(Enrollment).GetMethod("CompleteEnrollment", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Invoke(enrollment, null);
        
        string certificateTitle = "Certificate of Completion";

        // Act
        student.AddCertificate(courseId, certificateTitle);

        // Assert
        Assert.Single(student.Certificates);
        Assert.Equal(courseId, student.Certificates.First().CourseId);
        Assert.Equal(certificateTitle, student.Certificates.First().Title);
    }

    [Fact]
    public void Deactivate_ActiveStudent_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var student = CreateValidStudent();
        Assert.True(student.IsActive);

        // Act
        student.Deactivate();

        // Assert
        Assert.False(student.IsActive);
    }

    [Fact]
    public void Activate_InactiveStudent_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var student = CreateValidStudent();
        student.Deactivate();
        Assert.False(student.IsActive);

        // Act
        student.Activate();

        // Assert
        Assert.True(student.IsActive);
    }

    [Fact]
    public void CompleteCourse_ShouldUpdateDates()
    {
        // Arrange
        var student = CreateValidStudent();
        Guid courseId = Guid.NewGuid();
        var enrollment = student.EnrollInCourse(courseId, 99.99m);
        enrollment.ActivateEnrollment(); // Ativa a matrícula

        // Act
        var beforeUpdate = DateTime.UtcNow;
        student.CompleteCourse(courseId);
        var afterUpdate = DateTime.UtcNow;

        // Assert
        var completedEnrollment = student.Enrollments.First(e => e.CourseId == courseId);
        Assert.NotNull(completedEnrollment.CompletionDate);
        Assert.NotNull(completedEnrollment.UpdatedAt);
        Assert.True(completedEnrollment.CompletionDate >= beforeUpdate);
        Assert.True(completedEnrollment.CompletionDate <= afterUpdate);
        Assert.True(completedEnrollment.UpdatedAt >= beforeUpdate);
        Assert.True(completedEnrollment.UpdatedAt <= afterUpdate);
    }

    [Fact]
    public void RecordProgress_WithValidParameters_ShouldAddProgressToHistory()
    {
        // Arrange
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1));

        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        // Enroll student in the course first
        var enrollment = student.EnrollInCourse(courseId, 99.99m);
        
        // Precisamos ativar a matrícula primeiro
        enrollment.ActivateEnrollment();

        // Act
        student.RecordProgress(courseId, lessonId);

        // Assert
        Assert.NotNull(student.UpdatedAt);
        // Verificar se existe uma lição completa para o curso
        Assert.True(student.LearningHistory.HasCompletedLesson(courseId, lessonId));
    }

    [Fact]
    public void RecordProgress_WhenNotEnrolled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1));

        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => student.RecordProgress(courseId, lessonId));
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateStudent()
    {
        // Arrange
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1));
        
        student.Deactivate();
        Assert.False(student.IsActive);
        
        // Act
        student.Activate();

        // Assert
        Assert.True(student.IsActive);
        Assert.NotNull(student.UpdatedAt);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotChangeState()
    {
        // Arrange
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1));
        
        var originalUpdatedAt = student.UpdatedAt;
        Assert.True(student.IsActive);
        
        // Wait to ensure timestamps would be different
        System.Threading.Thread.Sleep(10);
        
        // Act
        student.Activate();

        // Assert
        Assert.True(student.IsActive);
        Assert.Equal(originalUpdatedAt, student.UpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateStudent()
    {
        // Arrange
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1));
        
        Assert.True(student.IsActive);
        
        // Act
        student.Deactivate();

        // Assert
        Assert.False(student.IsActive);
        Assert.NotNull(student.UpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotChangeState()
    {
        // Arrange
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1));
        
        student.Deactivate();
        var originalUpdatedAt = student.UpdatedAt;
        Assert.False(student.IsActive);
        
        // Wait to ensure timestamps would be different
        System.Threading.Thread.Sleep(10);
        
        // Act
        student.Deactivate();

        // Assert
        Assert.False(student.IsActive);
        Assert.Equal(originalUpdatedAt, student.UpdatedAt);
    }

    [Fact]
    public void FullName_ShouldReturnCombinedFirstAndLastName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var student = new Student(
            firstName,
            lastName,
            "john.doe@example.com",
            new DateTime(1990, 1, 1));

        // Act & Assert
        Assert.Equal($"{firstName} {lastName}", student.FullName);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateStudent()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var dateOfBirth = new DateTime(1990, 1, 1);
        var phoneNumber = "123456789";

        // Act
        var student = new Student(firstName, lastName, email, dateOfBirth, phoneNumber);

        // Assert
        Assert.Equal(firstName, student.FirstName);
        Assert.Equal(lastName, student.LastName);
        Assert.Equal(email, student.Email);
        Assert.Equal(dateOfBirth, student.DateOfBirth);
        Assert.Equal(phoneNumber, student.PhoneNumber);
        Assert.True(student.IsActive);
        Assert.NotNull(student.LearningHistory);
        Assert.Empty(student.Enrollments);
        Assert.Empty(student.Certificates);
        Assert.NotEqual(Guid.Empty, student.Id);
        Assert.NotNull(student.CreatedAt);
        Assert.NotNull(student.UpdatedAt);
    }

    [Theory]
    [InlineData("", "Doe", "john.doe@example.com")]
    [InlineData("John", "", "john.doe@example.com")]
    [InlineData("John", "Doe", "")]
    public void Constructor_WithInvalidParameters_ShouldThrowArgumentException(
        string firstName,
        string lastName,
        string email)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Student(
            firstName,
            lastName,
            email,
            new DateTime(1990, 1, 1)));
    }

    private Student CreateValidStudent()
    {
        return new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1),
            "123-456-7890"
        );
    }
} 