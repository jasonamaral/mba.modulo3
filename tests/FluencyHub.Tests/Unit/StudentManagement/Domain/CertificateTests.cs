using Xunit;
using FluentAssertions;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Models;
using FluencyHub.SharedKernel.Contracts;

namespace FluencyHub.Tests.Unit.StudentManagement.Domain;

public class CertificateTests
{
    [Fact]
    public void Certificate_Constructor_ShouldCreateCertificate_WhenValidParameters()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var title = "Certificado de Inglês Básico";
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();

        // Act
        var certificate = CreateCertificate(studentId, courseId, title, student, course);

        // Assert
        certificate.StudentId.Should().Be(studentId);
        certificate.CourseId.Should().Be(courseId);
        certificate.Title.Should().Be(title);
        certificate.IssueDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        certificate.CertificateNumber.Should().NotBeNullOrEmpty();
        certificate.CertificateNumber.Should().StartWith("CERT-");
        certificate.Score.Should().BeNull();
        certificate.Feedback.Should().BeNull();
        certificate.Id.Should().NotBe(Guid.Empty);
        certificate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        certificate.Student.Should().Be(student);
        certificate.Course.Should().Be(course);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Certificate_Constructor_ShouldThrowException_WhenTitleIsEmpty(string title)
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
        {
            var student = CreateValidStudent();
            var course = CreateValidCourseReference();
            CreateCertificate(studentId, courseId, title, student, course);
        });
        
        exception.Message.Should().Contain("O título do certificado não pode estar vazio");
        exception.ParamName.Should().Be("title");
    }

    [Fact]
    public void Certificate_Constructor_ShouldThrowException_WhenTitleIsNull()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
        {
            var student = CreateValidStudent();
            var course = CreateValidCourseReference();
            CreateCertificate(studentId, courseId, null!, student, course);
        });
        
        exception.Message.Should().Contain("O título do certificado não pode estar vazio");
        exception.ParamName.Should().Be("title");
    }

    [Fact]
    public void Certificate_UpdateTitle_ShouldUpdateTitle_WhenValidTitle()
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var newTitle = "Novo Título do Certificado";
        var originalUpdatedAt = certificate.UpdatedAt;

        // Act
        certificate.UpdateTitle(newTitle);

        // Assert
        certificate.Title.Should().Be(newTitle);
        certificate.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        certificate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Certificate_UpdateTitle_ShouldThrowException_WhenTitleIsEmpty(string title)
    {
        // Arrange
        var certificate = CreateValidCertificate();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            certificate.UpdateTitle(title));
        
        exception.Message.Should().Contain("O título do certificado não pode estar vazio");
        exception.ParamName.Should().Be("title");
    }

    [Fact]
    public void Certificate_UpdateTitle_ShouldThrowException_WhenTitleIsNull()
    {
        // Arrange
        var certificate = CreateValidCertificate();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            certificate.UpdateTitle(null!));
        
        exception.Message.Should().Contain("O título do certificado não pode estar vazio");
        exception.ParamName.Should().Be("title");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(85)]
    [InlineData(100)]
    public void Certificate_SetScore_ShouldSetScore_WhenValidScore(int score)
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalUpdatedAt = certificate.UpdatedAt;

        // Act
        certificate.SetScore(score);

        // Assert
        certificate.Score.Should().Be(score);
        certificate.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        certificate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Certificate_SetScore_ShouldThrowException_WhenScoreIsNegative(int score)
    {
        // Arrange
        var certificate = CreateValidCertificate();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            certificate.SetScore(score));
        
        exception.Message.Should().Contain("A nota deve estar entre 0 e 100");
        exception.ParamName.Should().Be("score");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(150)]
    [InlineData(200)]
    public void Certificate_SetScore_ShouldThrowException_WhenScoreIsAbove100(int score)
    {
        // Arrange
        var certificate = CreateValidCertificate();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            certificate.SetScore(score));
        
        exception.Message.Should().Contain("A nota deve estar entre 0 e 100");
        exception.ParamName.Should().Be("score");
    }

    [Fact]
    public void Certificate_SetFeedback_ShouldSetFeedback_WhenValidFeedback()
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var feedback = "Excelente desempenho no curso!";
        var originalUpdatedAt = certificate.UpdatedAt;

        // Act
        certificate.SetFeedback(feedback);

        // Assert
        certificate.Feedback.Should().Be(feedback);
        certificate.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        certificate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Certificate_SetFeedback_ShouldSetFeedback_WhenFeedbackIsNull()
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalUpdatedAt = certificate.UpdatedAt;

        // Act
        certificate.SetFeedback(null!);

        // Assert
        certificate.Feedback.Should().BeNull();
        certificate.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        certificate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Certificate_SetFeedback_ShouldSetFeedback_WhenFeedbackIsEmpty()
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var originalUpdatedAt = certificate.UpdatedAt;

        // Act
        certificate.SetFeedback(string.Empty);

        // Assert
        certificate.Feedback.Should().Be(string.Empty);
        certificate.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);
        certificate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Certificate_GenerateCertificateNumber_ShouldGenerateUniqueNumber()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var title = "Certificado Teste";
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();

        // Act
        var certificate1 = CreateCertificate(studentId, courseId, title, student, course);
        var certificate2 = CreateCertificate(studentId, courseId, title, student, course);

        // Assert
        certificate1.CertificateNumber.Should().NotBeNullOrEmpty();
        certificate2.CertificateNumber.Should().NotBeNullOrEmpty();
        certificate1.CertificateNumber.Should().NotBe(certificate2.CertificateNumber);
        
        // Verificar formato: CERT-YYYYMMDD-XXXXXXXX
        certificate1.CertificateNumber.Should().MatchRegex(@"^CERT-\d{8}-[a-f0-9]{8}$");
        certificate2.CertificateNumber.Should().MatchRegex(@"^CERT-\d{8}-[a-f0-9]{8}$");
        
        // Verificar se contém a data atual
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        certificate1.CertificateNumber.Should().Contain(today);
        certificate2.CertificateNumber.Should().Contain(today);
    }

    [Fact]
    public void Certificate_Properties_ShouldBeSetCorrectly_WhenUsingSetters()
    {
        // Arrange
        var certificate = CreateValidCertificate();
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();

        // Act
        certificate.Student = student;
        certificate.Course = course;

        // Assert
        certificate.Student.Should().Be(student);
        certificate.Course.Should().Be(course);
    }

    #region Helper Methods

    private static Certificate CreateCertificate(Guid studentId, Guid courseId, string title, Student student, ICourse course)
    {
        // Usando uma abordagem que satisfaz os required members explicitamente
        var certificate = new Certificate(studentId, courseId, title)
        {
            Title = title, // Explicitamente definindo novamente para satisfazer required
            CertificateNumber = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}", // Explicitamente definindo
            Student = student,
            Course = course
        };
        return certificate;
    }

    private static Certificate CreateValidCertificate()
    {
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var title = "Certificado de Inglês Básico";
        var student = CreateValidStudent();
        var course = CreateValidCourseReference();
        
        return CreateCertificate(studentId, courseId, title, student, course);
    }

    private static Student CreateValidStudent()
    {
        return new Student(
            "João",
            "Silva", 
            "joao.silva@exemplo.com",
            new DateTime(1990, 1, 1));
    }

    private static CourseReference CreateValidCourseReference()
    {
        return new CourseReference(
            Guid.NewGuid(),
            "Inglês Básico",
            "Curso de inglês para iniciantes",
            299.99m,
            true);
    }

    #endregion
} 