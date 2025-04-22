using FluencyHub.API.Controllers;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.CompleteLessonForStudent;
using FluencyHub.Application.StudentManagement.Queries.GetAllStudents;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.StudentManagement;
using FluencyHub.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FluencyHub.Tests.API.Controllers;

public class StudentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly StudentsController _controller;
    
    public StudentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        
        // Mock do UserManager
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null, null, null, null, null, null, null, null);
        
        _controller = new StudentsController(_mediatorMock.Object, _userManagerMock.Object);
    }
    
    [Fact]
    public async Task GetAllStudents_ReturnsOkWithStudentsList()
    {
        // Arrange
        var students = new List<StudentDto>
        {
            new StudentDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Student",
                LastName = "Silva",
                Email = "student1@example.com",
                Phone = "+1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                FirstName = "Student",
                LastName = "Pereira",
                Email = "student2@example.com",
                Phone = "+1234567891",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllStudentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);
        
        // Act
        var result = await _controller.GetAllStudents();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStudents = Assert.IsAssignableFrom<IEnumerable<StudentDto>>(okResult.Value);
        Assert.Equal(2, returnedStudents.Count());
        Assert.Equal("Silva", returnedStudents.First().LastName);
        Assert.Equal("Pereira", returnedStudents.Last().LastName);
    }
    
    [Fact]
    public async Task GetAllStudents_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllStudentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error fetching students"));
        
        // Act
        var result = await _controller.GetAllStudents();
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Error fetching students", badRequestResult.Value);
    }
    
    [Fact]
    public async Task MarkLessonAsCompleted_WithValidData_ReturnsOkWithResult()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var commandResult = new CompleteLessonForStudentResult
        {
            Success = true,
            Message = "Lesson marked as completed successfully"
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CompleteLessonForStudentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResult);
        
        // Act
        var result = await _controller.MarkLessonAsCompleted(studentId, courseId, lessonId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<CompleteLessonForStudentResult>(okResult.Value);
        Assert.True(returnedResult.Success);
        Assert.Equal("Lesson marked as completed successfully", returnedResult.Message);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<CompleteLessonForStudentCommand>(c => 
                c.StudentId == studentId && 
                c.CourseId == courseId && 
                c.LessonId == lessonId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task MarkLessonAsCompleted_WithNotFoundEntity_ReturnsNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CompleteLessonForStudentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Student", studentId));
        
        // Act
        var result = await _controller.MarkLessonAsCompleted(studentId, courseId, lessonId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Entity \"Student\" ({studentId}) was not found.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task MarkLessonAsCompleted_WithBadRequest_ReturnsBadRequest()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CompleteLessonForStudentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Student is not enrolled in this course"));
        
        // Act
        var result = await _controller.MarkLessonAsCompleted(studentId, courseId, lessonId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Student is not enrolled in this course", badRequestResult.Value);
    }
    
    [Fact]
    public async Task MarkLessonAsCompleted_WithGenericException_ReturnsBadRequest()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CompleteLessonForStudentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An unexpected error occurred"));
        
        // Act
        var result = await _controller.MarkLessonAsCompleted(studentId, courseId, lessonId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("An unexpected error occurred", badRequestResult.Value);
    }
} 