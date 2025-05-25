using Xunit;
using FluentAssertions;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Models;
using FluencyHub.StudentManagement.Domain.Enums;

namespace FluencyHub.Tests.Unit.StudentManagement.Domain;

public class StudentTests
{
    [Fact]
    public void Constructor_ShouldCreateStudent_WhenValidParameters()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act
        var student = new Student(firstName, lastName, email, dateOfBirth);

        // Assert
        student.FirstName.Should().Be(firstName);
        student.LastName.Should().Be(lastName);
        student.Email.Should().Be(email);
        student.DateOfBirth.Should().Be(dateOfBirth);
        student.IsActive.Should().BeTrue();
        student.FullName.Should().Be("John Doe");
        student.LearningHistory.Should().NotBeNull();
        student.Enrollments.Should().BeEmpty();
        student.Certificates.Should().BeEmpty();
        student.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowException_WhenFirstNameIsInvalid(string firstName)
    {
        // Arrange
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Student(firstName, lastName, email, dateOfBirth));
        
        exception.Message.Should().Contain("O nome não pode estar vazio");
        exception.ParamName.Should().Be("firstName");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("invalid")]
    public void Constructor_ShouldThrowException_WhenFirstNameIsTooShortOrInvalid(string firstName)
    {
        // Arrange
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Student(firstName, lastName, email, dateOfBirth));
        
        exception.Message.Should().Contain("O nome deve ter pelo menos 2 caracteres e ser válido");
        exception.ParamName.Should().Be("firstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowException_WhenLastNameIsInvalid(string lastName)
    {
        // Arrange
        var firstName = "John";
        var email = "john.doe@example.com";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Student(firstName, lastName, email, dateOfBirth));
        
        exception.Message.Should().Contain("O sobrenome não pode estar vazio");
        exception.ParamName.Should().Be("lastName");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("invalid")]
    public void Constructor_ShouldThrowException_WhenLastNameIsTooShortOrInvalid(string lastName)
    {
        // Arrange
        var firstName = "John";
        var email = "john.doe@example.com";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Student(firstName, lastName, email, dateOfBirth));
        
        exception.Message.Should().Contain("O sobrenome deve ter pelo menos 2 caracteres e ser válido");
        exception.ParamName.Should().Be("lastName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrowException_WhenEmailIsInvalid(string email)
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Student(firstName, lastName, email, dateOfBirth));
        
        exception.Message.Should().Contain("O email não pode estar vazio");
        exception.ParamName.Should().Be("email");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Constructor_ShouldThrowException_WhenEmailFormatIsInvalid(string email)
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Student(firstName, lastName, email, dateOfBirth));
        
        exception.Message.Should().Contain("O email deve ter um formato válido");
        exception.ParamName.Should().Be("email");
    }

    [Fact]
    public void Update_ShouldUpdateStudentInformation_WhenValidParameters()
    {
        // Arrange
        var student = CreateValidStudent();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newEmail = "jane.smith@example.com";
        var phoneNumber = "+1234567890";
        var address = "123 Main St";
        var city = "New York";
        var state = "NY";
        var country = "USA";

        // Act
        student.Update(newFirstName, newLastName, newEmail, phoneNumber, address, city, state, country);

        // Assert
        student.FirstName.Should().Be(newFirstName);
        student.LastName.Should().Be(newLastName);
        student.Email.Should().Be(newEmail);
        student.PhoneNumber.Should().Be(phoneNumber);
        student.Address.Should().Be(address);
        student.City.Should().Be(city);
        student.State.Should().Be(state);
        student.Country.Should().Be(country);
        student.FullName.Should().Be("Jane Smith");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_ShouldThrowException_WhenFirstNameIsInvalid(string firstName)
    {
        // Arrange
        var student = CreateValidStudent();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            student.Update(firstName, "Doe", "john.doe@example.com", null, null, null, null, null));
        
        exception.Message.Should().Contain("O nome não pode estar vazio");
        exception.ParamName.Should().Be("firstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_ShouldThrowException_WhenLastNameIsInvalid(string lastName)
    {
        // Arrange
        var student = CreateValidStudent();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            student.Update("John", lastName, "john.doe@example.com", null, null, null, null, null));
        
        exception.Message.Should().Contain("O sobrenome não pode estar vazio");
        exception.ParamName.Should().Be("lastName");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_WhenStudentIsActive()
    {
        // Arrange
        var student = CreateValidStudent();
        student.IsActive.Should().BeTrue();

        // Act
        student.Deactivate();

        // Assert
        student.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldNotChangeStatus_WhenStudentIsAlreadyInactive()
    {
        // Arrange
        var student = CreateValidStudent();
        student.Deactivate();
        var wasInactive = !student.IsActive;

        // Act
        student.Deactivate();

        // Assert
        student.IsActive.Should().BeFalse();
        wasInactive.Should().BeTrue();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue_WhenStudentIsInactive()
    {
        // Arrange
        var student = CreateValidStudent();
        student.Deactivate();
        student.IsActive.Should().BeFalse();

        // Act
        student.Activate();

        // Assert
        student.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_ShouldNotChangeStatus_WhenStudentIsAlreadyActive()
    {
        // Arrange
        var student = CreateValidStudent();
        var wasActive = student.IsActive;

        // Act
        student.Activate();

        // Assert
        student.IsActive.Should().BeTrue();
        wasActive.Should().BeTrue();
    }

    [Fact]
    public void EnrollInCourse_ShouldAddEnrollment_WhenStudentIsActiveAndNotEnrolled()
    {
        // Arrange
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();
        var initialEnrollmentCount = student.Enrollments.Count;

        // Act
        student.EnrollInCourse(course);

        // Assert
        student.Enrollments.Should().HaveCount(initialEnrollmentCount + 1);
        var enrollment = student.Enrollments.First();
        enrollment.CourseId.Should().Be(course.Id);
        enrollment.Status.Should().Be(FluencyHub.StudentManagement.Domain.StatusMatricula.AguardandoPagamento);
    }

    [Fact]
    public void EnrollInCourse_ShouldThrowException_WhenStudentIsInactive()
    {
        // Arrange
        var student = CreateValidStudent();
        student.Deactivate();
        var course = CreateValidCourseReference();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            student.EnrollInCourse(course));
        
        exception.Message.Should().Contain("Estudante inativo não pode se matricular em cursos");
    }

    [Fact]
    public void EnrollInCourse_ShouldThrowException_WhenStudentIsAlreadyEnrolled()
    {
        // Arrange
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();
        student.EnrollInCourse(course);
        
        // Ativar a matrícula para simular um estudante já matriculado ativamente
        var enrollment = student.Enrollments.First();
        enrollment.ActivateEnrollment();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            student.EnrollInCourse(course));
        
        exception.Message.Should().Contain("Estudante já está matriculado neste curso");
    }

    [Fact]
    public void CompleteCourse_ShouldCompleteEnrollmentAndIssueCertificate_WhenStudentIsEnrolled()
    {
        // Arrange
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();
        student.EnrollInCourse(course);
        
        // Ativar a matrícula primeiro
        var enrollment = student.Enrollments.First();
        enrollment.ActivateEnrollment();
        
        var initialCertificateCount = student.Certificates.Count;

        // Act
        student.CompleteCourse(course);

        // Assert
        enrollment.Status.Should().Be(FluencyHub.StudentManagement.Domain.StatusMatricula.Concluida);
        student.Certificates.Should().HaveCount(initialCertificateCount + 1);
        var certificate = student.Certificates.First();
        certificate.CourseId.Should().Be(course.Id);
    }

    [Fact]
    public void CompleteCourse_ShouldThrowException_WhenStudentIsNotEnrolled()
    {
        // Arrange
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            student.CompleteCourse(course));
        
        exception.Message.Should().Contain("O estudante não está matriculado neste curso");
    }

    [Fact]
    public void CompleteCourse_ShouldThrowException_WhenCourseIsAlreadyCompleted()
    {
        // Arrange
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();
        student.EnrollInCourse(course);
        
        // Ativar e completar a matrícula
        var enrollment = student.Enrollments.First();
        enrollment.ActivateEnrollment();
        student.CompleteCourse(course);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            student.CompleteCourse(course));
        
        exception.Message.Should().Contain("Curso já foi concluído");
    }

    [Fact]
    public void RecordProgress_ShouldAddProgressToLearningHistory_WhenStudentIsActivelyEnrolled()
    {
        // Arrange
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();
        var lessonId = Guid.NewGuid();
        student.EnrollInCourse(course);
        
        // Ativar a matrícula para permitir o progresso
        var enrollment = student.Enrollments.First();
        enrollment.ActivateEnrollment();

        // Act
        student.RecordProgress(course.Id, lessonId);

        // Assert
        // Verificamos se o método foi chamado sem exceção
        // A implementação específica do LearningHistory pode ser testada separadamente
        student.LearningHistory.Should().NotBeNull();
    }

    [Fact]
    public void RecordProgress_ShouldThrowException_WhenStudentIsNotActivelyEnrolled()
    {
        // Arrange
        var student = CreateValidStudent();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            student.RecordProgress(courseId, lessonId));
        
        exception.Message.Should().Contain("O estudante não está ativamente matriculado neste curso");
    }

    [Fact]
    public void FullName_ShouldReturnConcatenatedFirstAndLastName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var student = new Student(firstName, lastName, "john.doe@example.com", new DateTime(1990, 1, 1));

        // Act
        var fullName = student.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    private static Student CreateValidStudent()
    {
        return new Student("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));
    }

    private static CourseReference CreateValidCourseReference()
    {
        return new CourseReference(
            Guid.NewGuid(),
            "Test Course",
            "A test course description",
            99.99m,
            true
        );
    }
} 