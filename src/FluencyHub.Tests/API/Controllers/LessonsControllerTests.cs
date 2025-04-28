using FluencyHub.API.Controllers;
using FluencyHub.API.Models;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Commands.AddLesson;
using FluencyHub.Application.ContentManagement.Commands.DeleteLesson;
using FluencyHub.Application.ContentManagement.Commands.UpdateLesson;
using FluencyHub.Application.ContentManagement.Queries.GetLessonsByCourse;
using FluencyHub.Application.StudentManagement.Commands.CompleteLessonForStudent;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluencyHub.Tests.API.Controllers;

public class LessonsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly LessonsController _controller;
    
    public LessonsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _controller = new LessonsController(_mediatorMock.Object, _httpContextAccessorMock.Object);
        
        // Configurar o HttpContext para os testes
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }
    
    [Fact]
    public async Task GetLessonsByCourse_WithValidId_ReturnsOkWithLessons()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessons = new List<LessonDto>
        {
            new LessonDto
            {
                Id = Guid.NewGuid(),
                Title = "Lesson 1",
                Content = "Content 1",
                Order = 1
            },
            new LessonDto
            {
                Id = Guid.NewGuid(),
                Title = "Lesson 2",
                Content = "Content 2",
                Order = 2
            }
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetLessonsByCourseQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessons);
        
        // Act
        var result = await _controller.GetLessonsByCourse(courseId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLessons = Assert.IsAssignableFrom<IEnumerable<LessonDto>>(okResult.Value);
        Assert.Equal(2, returnedLessons.Count());
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetLessonsByCourseQuery>(q => q.CourseId == courseId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task GetLessonsByCourse_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetLessonsByCourseQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Course not found"));
        
        // Act
        var result = await _controller.GetLessonsByCourse(courseId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Course not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task AddLesson_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new LessonCreateRequest
        {
            Title = "New Lesson",
            Content = "New Content",
            Order = 1
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddLessonCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lessonId);
        
        // Act
        var result = await _controller.AddLesson(courseId, request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetLessonsByCourse), createdResult.ActionName);
        Assert.Equal(courseId, createdResult.RouteValues["courseId"]);
        
        // Usando Json para verificar a estrutura do objeto retornado
        var serializedValue = JsonSerializer.Serialize(createdResult.Value);
        var jsonDoc = JsonDocument.Parse(serializedValue);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("id", out var idProperty));
        Assert.Equal(lessonId.ToString(), idProperty.GetString());
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<AddLessonCommand>(c => 
                c.CourseId == courseId && 
                c.Title == request.Title && 
                c.Content == request.Content),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task AddLesson_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new LessonCreateRequest
        {
            Title = "New Lesson",
            Content = "New Content",
            Order = 1
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddLessonCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid data"));
        
        // Act
        var result = await _controller.AddLesson(courseId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid data", badRequestResult.Value);
    }
    
    [Fact]
    public async Task UpdateLesson_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var command = new UpdateLessonCommand
        {
            CourseId = courseId,
            LessonId = lessonId,
            Title = "Updated Lesson",
            Content = "Updated Content"
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateLessonCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.UpdateLesson(courseId, lessonId, command);
        
        // Assert
        Assert.IsType<OkResult>(result);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateLessonCommand>(c => 
                c.CourseId == courseId && 
                c.LessonId == lessonId && 
                c.Title == command.Title &&
                c.Content == command.Content),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateLesson_WithMismatchingIds_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var command = new UpdateLessonCommand
        {
            CourseId = Guid.NewGuid(), // Different course ID
            LessonId = Guid.NewGuid(), // Different lesson ID
            Title = "Updated Lesson",
            Content = "Updated Content"
        };
        
        // Act
        var result = await _controller.UpdateLesson(courseId, lessonId, command);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Course ID and Lesson ID in the route must match the IDs in the command", badRequestResult.Value);
    }
    
    [Fact]
    public async Task UpdateLesson_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var command = new UpdateLessonCommand
        {
            CourseId = courseId,
            LessonId = lessonId,
            Title = "Updated Lesson",
            Content = "Updated Content"
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateLessonCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid data"));
        
        // Act
        var result = await _controller.UpdateLesson(courseId, lessonId, command);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid data", badRequestResult.Value);
    }

    [Fact]
    public async Task CompleteLesson_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        // Configurar o usuário no HttpContext
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, studentId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(principal);
        
        _mediatorMock.Setup(x => x.Send(It.Is<CompleteLessonForStudentCommand>(cmd => 
            cmd.StudentId == studentId && 
            cmd.CourseId == courseId && 
            cmd.LessonId == lessonId), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompleteLessonForStudentResult 
            { 
                Success = true, 
                StudentId = studentId,
                CourseId = courseId,
                LessonId = lessonId,
                Message = "Lesson marked as completed successfully."
            });

        // Act
        var result = await _controller.CompleteLesson(courseId, lessonId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<CompleteLessonForStudentResult>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("Lesson marked as completed successfully.", returnValue.Message);
        _mediatorMock.Verify(x => x.Send(It.IsAny<CompleteLessonForStudentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteLesson_WhenStudentNotEnrolled_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        // Configurar o usuário no HttpContext
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, studentId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(principal);
        
        _mediatorMock.Setup(x => x.Send(It.IsAny<CompleteLessonForStudentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Student is not enrolled in this course."));

        // Act
        var result = await _controller.CompleteLesson(courseId, lessonId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Student is not enrolled in this course.", badRequestResult.Value);
    }

    [Fact]
    public async Task CompleteLesson_WithNonExistentLesson_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        // Configurar o usuário no HttpContext
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, studentId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(principal);
        
        _mediatorMock.Setup(x => x.Send(It.IsAny<CompleteLessonForStudentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Lesson not found."));

        // Act
        var result = await _controller.CompleteLesson(courseId, lessonId, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Lesson not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteLesson_WithValidIds_ReturnsOkResult()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteLessonCommand>(c => c.CourseId == courseId && c.LessonId == lessonId),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.DeleteLesson(courseId, lessonId);
        
        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteLessonCommand>(c => c.CourseId == courseId && c.LessonId == lessonId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteLesson_LessonNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteLessonCommand>(c => c.CourseId == courseId && c.LessonId == lessonId),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Lesson", lessonId));
        
        // Act
        var result = await _controller.DeleteLesson(courseId, lessonId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        Assert.Contains($"Entity \"Lesson\" ({lessonId})", notFoundResult.Value.ToString());
    }
    
    [Fact]
    public async Task DeleteLesson_WithError_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var errorMessage = "An error occurred while deleting the lesson";
        
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteLessonCommand>(c => c.CourseId == courseId && c.LessonId == lessonId),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));
        
        // Act
        var result = await _controller.DeleteLesson(courseId, lessonId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }

    [Fact]
    public async Task CompleteLesson_WithCompletedFalse_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = false };
        
        // Act
        var result = await _controller.CompleteLesson(courseId, lessonId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The 'Completed' field must be true to mark a lesson as completed.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task CompleteLesson_WithMissingStudentId_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        // Configurar HttpContext sem claims de identidade do usuário
        _httpContextAccessorMock.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim)null);
        
        // Act
        var result = await _controller.CompleteLesson(courseId, lessonId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Student ID could not be determined from the authentication token.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task CompleteLesson_WithInvalidStudentId_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        // Configurar HttpContext com um valor de ID de estudante inválido
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(principal);
        
        // Act
        var result = await _controller.CompleteLesson(courseId, lessonId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Student ID could not be determined from the authentication token.", badRequestResult.Value);
    }
} 