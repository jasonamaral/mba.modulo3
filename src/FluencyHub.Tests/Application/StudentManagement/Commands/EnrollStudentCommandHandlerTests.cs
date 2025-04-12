using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.StudentManagement.Commands.EnrollStudent;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.StudentManagement;
using Moq;

namespace FluencyHub.Tests.Application.StudentManagement.Commands;

public class EnrollStudentCommandHandlerTests
{
    private readonly Mock<FluencyHub.Application.Common.Interfaces.IStudentRepository> _mockStudentRepository;
    private readonly Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository> _mockCourseRepository;
    private readonly EnrollStudentCommandHandler _handler;

    public EnrollStudentCommandHandlerTests()
    {
        _mockStudentRepository = new Mock<FluencyHub.Application.Common.Interfaces.IStudentRepository>();
        _mockCourseRepository = new Mock<FluencyHub.Application.Common.Interfaces.ICourseRepository>();
        _handler = new EnrollStudentCommandHandler(
            _mockStudentRepository.Object,
            _mockCourseRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEnrollmentAndReturnId()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand(studentId, courseId);

        var student = CreateStudent(studentId);
        var course = CreateCourse(courseId, 99.99m);

        _mockStudentRepository.Setup(r => r.GetByIdAsync(studentId))
            .ReturnsAsync(student);
        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        Guid enrollmentId = Guid.Empty;
        _mockStudentRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => 
            {
                if (student.Enrollments.Any())
                    enrollmentId = student.Enrollments.First().Id;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(enrollmentId, result);
        Assert.Single(student.Enrollments);
        Assert.Equal(courseId, student.Enrollments.First().CourseId);
        Assert.Equal(studentId, student.Enrollments.First().StudentId);
        Assert.Equal(course.Price, student.Enrollments.First().Price);
        Assert.Equal(EnrollmentStatus.PendingPayment, student.Enrollments.First().Status);
        
        _mockStudentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StudentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand(studentId, courseId);

        _mockStudentRepository.Setup(r => r.GetByIdAsync(studentId))
            .ReturnsAsync((Student)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _mockStudentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CourseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollStudentCommand(studentId, courseId);

        var student = CreateStudent(studentId);

        _mockStudentRepository.Setup(r => r.GetByIdAsync(studentId))
            .ReturnsAsync(student);
        _mockCourseRepository.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync((Course)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _mockStudentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Student CreateStudent(Guid id)
    {
        var student = new Student(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(1990, 1, 1),
            "123-456-7890"
        );
        
        // Use reflection to set the ID
        typeof(Student).GetProperty("Id").SetValue(student, id);
        
        return student;
    }

    private Course CreateCourse(Guid id, decimal price)
    {
        var courseContent = new CourseContent(
            "Syllabus",
            "Learning Objectives",
            "Prerequisites",
            "Target Audience",
            "English",
            "A1"
        );
        
        var course = new Course(
            "English for Beginners",
            "A comprehensive course for beginners",
            courseContent,
            price
        );
        
        // Use reflection to set the ID
        typeof(Course).GetProperty("Id").SetValue(course, id);
        
        return course;
    }
} 