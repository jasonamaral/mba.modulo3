using FluencyHub.StudentManagement.Application.Commands.GenerateCertificate;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using FluencyHub.SharedKernel.Contracts;
using Moq;
using Xunit;
using FluentAssertions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Commands;

public class GenerateCertificateCommandHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<ICertificateRepository> _mockCertificateRepository;
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly GenerateCertificateCommandHandler _handler;

    public GenerateCertificateCommandHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockCertificateRepository = new Mock<ICertificateRepository>();
        _mockCourseRepository = new Mock<ICourseRepository>();
        
        _handler = new GenerateCertificateCommandHandler(
            _mockStudentRepository.Object,
            _mockCertificateRepository.Object,
            _mockCourseRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCertificate_WhenValidCommand()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var student = new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));
        var course = new CourseInfo 
        { 
            Id = courseId,
            Name = "Curso de Inglês", 
            Description = "Curso completo de inglês", 
            Price = 100m 
        };
        
        var command = new GenerateCertificateCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            IssueDate = DateTime.UtcNow,
            Score = 85,
            Feedback = "Excelente desempenho"
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockCourseRepository
            .Setup(x => x.ExistsAsync(courseId))
            .ReturnsAsync(true);

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _mockCertificateRepository
            .Setup(x => x.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync((Certificate?)null);

        _mockCertificateRepository
            .Setup(x => x.AddAsync(It.IsAny<Certificate>()))
            .Returns(Task.CompletedTask);

        _mockCertificateRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        
        _mockCertificateRepository.Verify(x => x.AddAsync(It.IsAny<Certificate>()), Times.Once);
        _mockCertificateRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStudentNotFound()
    {
        // Arrange
        var command = new GenerateCertificateCommand
        {
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            IssueDate = DateTime.UtcNow,
            Score = 85
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(command.StudentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Estudante com ID {command.StudentId} não encontrado");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCourseNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var student = new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));
        
        var command = new GenerateCertificateCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            IssueDate = DateTime.UtcNow,
            Score = 85
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockCourseRepository
            .Setup(x => x.ExistsAsync(courseId))
            .ReturnsAsync(false);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Curso com ID {courseId} não encontrado");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCertificateAlreadyExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var student = new Student("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1));
        
        var course = new CourseInfo 
        { 
            Id = courseId,
            Name = "Existing Course", 
            Description = "Test", 
            Price = 100m 
        };
        
        var mockCourse = new Mock<ICourse>();
        mockCourse.Setup(x => x.Id).Returns(courseId);
        mockCourse.Setup(x => x.Name).Returns("Existing Course");
        mockCourse.Setup(x => x.Description).Returns("Test");
        mockCourse.Setup(x => x.Price).Returns(100m);
        
        var existingCertificate = new Certificate(studentId, courseId, "Existing Certificate")
        {
            Student = student,
            Course = mockCourse.Object,
            Title = "Existing Certificate",
            CertificateNumber = "CERT-TEST-001"
        };
        
        var command = new GenerateCertificateCommand
        {
            StudentId = studentId,
            CourseId = courseId,
            IssueDate = DateTime.UtcNow,
            Score = 85
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockCourseRepository
            .Setup(x => x.ExistsAsync(courseId))
            .ReturnsAsync(true);

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _mockCertificateRepository
            .Setup(x => x.GetByStudentAndCourseAsync(studentId, courseId))
            .ReturnsAsync(existingCertificate);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Certificate already exists for student {studentId} and course {courseId}");
    }
} 