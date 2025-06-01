using FluentAssertions;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Domain.Enums;
using FluencyHub.SharedKernel.Contracts;
using Moq;
using Xunit;

namespace FluencyHub.Tests.Unit.StudentManagement.Domain;

public class EnrollmentTests
{
    private Student CreateValidStudent()
    {
        return new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));
    }

    private Mock<ICourse> CreateMockCourse()
    {
        var mockCourse = new Mock<ICourse>();
        mockCourse.Setup(c => c.Id).Returns(Guid.NewGuid());
        mockCourse.Setup(c => c.Name).Returns("Curso de Inglês");
        mockCourse.Setup(c => c.Description).Returns("Curso completo de inglês");
        mockCourse.Setup(c => c.Language).Returns("Português");
        mockCourse.Setup(c => c.Level).Returns("Iniciante");
        mockCourse.Setup(c => c.Price).Returns(299.99m);
        mockCourse.Setup(c => c.IsActive).Returns(true);
        return mockCourse;
    }

    private Enrollment CreateValidEnrollment()
    {
        var student = CreateValidStudent();
        var mockCourse = CreateMockCourse();
        return new Enrollment(Guid.NewGuid(), Guid.NewGuid(), 100m)
        {
            Student = student,
            Course = mockCourse.Object
        };
    }

    [Fact]
    public void Enrollment_Constructor_ShouldCreateValidEnrollment()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var price = 299.99m;
        var student = CreateValidStudent();
        var mockCourse = CreateMockCourse();

        // Act
        var enrollment = new Enrollment(studentId, courseId, price)
        {
            Student = student,
            Course = mockCourse.Object
        };

        // Assert
        enrollment.StudentId.Should().Be(studentId);
        enrollment.CourseId.Should().Be(courseId);
        enrollment.Price.Should().Be(price);
        enrollment.Status.Should().Be(StatusMatricula.AguardandoPagamento);
        enrollment.EnrollmentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        enrollment.ActivationDate.Should().BeNull();
        enrollment.CompletionDate.Should().BeNull();
        enrollment.PaymentId.Should().BeNull();
        enrollment.IsPendingPayment.Should().BeTrue();
        enrollment.IsActive.Should().BeFalse();
        enrollment.IsCompleted.Should().BeFalse();
        enrollment.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public void Enrollment_Constructor_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var price = -10m;
        var student = CreateValidStudent();
        var mockCourse = CreateMockCourse();

        // Act & Assert
        var action = () => new Enrollment(studentId, courseId, price)
        {
            Student = student,
            Course = mockCourse.Object
        };
        action.Should().Throw<ArgumentException>()
            .WithMessage("O preço não pode ser negativo*");
    }

    [Fact]
    public void ActivateEnrollment_WhenPendingPayment_ShouldActivateEnrollment()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();

        // Act
        enrollment.ActivateEnrollment();

        // Assert
        enrollment.Status.Should().Be(StatusMatricula.Ativa);
        enrollment.ActivationDate.Should().NotBeNull();
        enrollment.ActivationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        enrollment.IsActive.Should().BeTrue();
        enrollment.IsPendingPayment.Should().BeFalse();
    }

    [Fact]
    public void ActivateEnrollment_WhenNotPendingPayment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();
        enrollment.ActivateEnrollment(); // Já ativa

        // Act & Assert
        var action = () => enrollment.ActivateEnrollment();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível ativar uma matrícula com status Ativa");
    }

    [Fact]
    public void CompleteEnrollment_WhenActive_ShouldCompleteEnrollment()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();
        enrollment.ActivateEnrollment();

        // Act
        enrollment.CompleteEnrollment();

        // Assert
        enrollment.Status.Should().Be(StatusMatricula.Concluida);
        enrollment.CompletionDate.Should().NotBeNull();
        enrollment.CompletionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        enrollment.IsCompleted.Should().BeTrue();
        enrollment.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CompleteEnrollment_WhenNotActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();

        // Act & Assert
        var action = () => enrollment.CompleteEnrollment();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível completar uma matrícula com status AguardandoPagamento. A matrícula deve estar ativa.");
    }

    [Fact]
    public void CancelEnrollment_WhenNotCompleted_ShouldCancelEnrollment()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();

        // Act
        enrollment.CancelEnrollment();

        // Assert
        enrollment.Status.Should().Be(StatusMatricula.Cancelada);
        enrollment.IsCancelled.Should().BeTrue();
        enrollment.IsActive.Should().BeFalse();
        enrollment.IsPendingPayment.Should().BeFalse();
    }

    [Fact]
    public void CancelEnrollment_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();
        enrollment.ActivateEnrollment();
        enrollment.CompleteEnrollment();

        // Act & Assert
        var action = () => enrollment.CancelEnrollment();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível cancelar uma matrícula concluída");
    }

    [Fact]
    public void ProcessPaymentSuccess_WhenPendingPayment_ShouldActivateEnrollment()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();
        var paymentId = Guid.NewGuid();
        var amount = 100m;

        // Act
        enrollment.ProcessPaymentSuccess(paymentId, amount);

        // Assert
        enrollment.PaymentId.Should().Be(paymentId);
        enrollment.Status.Should().Be(StatusMatricula.Ativa);
        enrollment.ActivationDate.Should().NotBeNull();
        enrollment.ActivationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        enrollment.IsActive.Should().BeTrue();
        enrollment.IsPendingPayment.Should().BeFalse();
    }

    [Fact]
    public void ProcessPaymentSuccess_WhenNotPendingPayment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();
        enrollment.ActivateEnrollment(); // Já ativa
        var paymentId = Guid.NewGuid();
        var amount = 100m;

        // Act & Assert
        var action = () => enrollment.ProcessPaymentSuccess(paymentId, amount);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível processar o pagamento para uma matrícula com status Ativa");
    }

    [Fact]
    public void ProcessPaymentFailure_WhenPendingPayment_ShouldSetFailureReason()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();
        var errorMessage = "Cartão recusado";

        // Act
        enrollment.ProcessPaymentFailure(errorMessage);

        // Assert
        enrollment.PaymentFailureReason.Should().Be(errorMessage);
        enrollment.Status.Should().Be(StatusMatricula.AguardandoPagamento); // Status não muda
        enrollment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ProcessPaymentFailure_WhenNotPendingPayment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var enrollment = CreateValidEnrollment();
        enrollment.ActivateEnrollment(); // Já ativa
        var errorMessage = "Cartão recusado";

        // Act & Assert
        var action = () => enrollment.ProcessPaymentFailure(errorMessage);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Não é possível processar a falha de pagamento para uma matrícula com status Ativa");
    }

    [Fact]
    public void Enrollment_WithRequiredProperties_ShouldWork()
    {
        // Arrange
        var student = CreateValidStudent();
        var mockCourse = CreateMockCourse();
        var enrollment = new Enrollment(student.Id, mockCourse.Object.Id, 100m)
        {
            Student = student,
            Course = mockCourse.Object
        };

        // Act & Assert
        enrollment.Student.Should().Be(student);
        enrollment.Course.Should().Be(mockCourse.Object);
        enrollment.StudentId.Should().Be(student.Id);
        enrollment.CourseId.Should().Be(mockCourse.Object.Id);
    }
} 