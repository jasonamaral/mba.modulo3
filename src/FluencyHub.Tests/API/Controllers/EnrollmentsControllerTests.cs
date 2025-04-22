using FluencyHub.API.Controllers;
using FluencyHub.API.Models;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Commands.CompleteEnrollment;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Application.StudentManagement.Commands.CompleteLesson;
using FluencyHub.Application.StudentManagement.Commands.EnrollStudent;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentEnrollments;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.StudentManagement;
using FluencyHub.Infrastructure.Persistence;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;

namespace FluencyHub.Tests.API.Controllers;

public class EnrollmentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly FluencyHubDbContext _dbContext;
    private readonly EnrollmentsController _controller;
    
    public EnrollmentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        
        // Criar um banco de dados em memória para testes
        var options = new DbContextOptionsBuilder<FluencyHubDbContext>()
            .UseInMemoryDatabase(databaseName: $"EnrollmentsControllerTests_{Guid.NewGuid()}")
            .Options;
        _dbContext = new FluencyHubDbContext(options);
        
        _controller = new EnrollmentsController(_mediatorMock.Object, _dbContext);
    }
    
    [Fact]
    public async Task EnrollStudent_WithValidCommand_ReturnsCreatedAtAction()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new EnrollmentCreateRequest
        {
            StudentId = studentId,
            CourseId = courseId
        };
        
        var enrollmentId = Guid.NewGuid();
        var expectedCommand = new EnrollStudentCommand(studentId, courseId);
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<EnrollStudentCommand>(c => 
                c.StudentId == studentId && c.CourseId == courseId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollmentId);
            
        var enrollmentDto = new EnrollmentDto
        {
            Id = enrollmentId,
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            Status = "Pending"
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollmentDto);
        
        // Act
        var result = await _controller.EnrollStudent(request);
        
        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(EnrollmentsController.GetEnrollment), createdAtActionResult.ActionName);
        Assert.Equal(enrollmentId, createdAtActionResult.RouteValues["id"]);
        Assert.Equal(enrollmentDto, createdAtActionResult.Value);
    }
    
    [Fact]
    public async Task EnrollStudent_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new EnrollmentCreateRequest
        {
            StudentId = studentId,
            CourseId = courseId
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<EnrollStudentCommand>(c => 
                c.StudentId == studentId && c.CourseId == courseId), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Course", courseId));
        
        // Act
        var result = await _controller.EnrollStudent(request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(courseId.ToString(), notFoundResult.Value.ToString());
    }
    
    [Fact]
    public async Task EnrollStudent_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new EnrollmentCreateRequest
        {
            StudentId = studentId,
            CourseId = courseId
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<EnrollStudentCommand>(c => 
                c.StudentId == studentId && c.CourseId == courseId), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Invalid enrollment data"));
        
        // Act
        var result = await _controller.EnrollStudent(request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        // Não precisamos verificar a propriedade específica, apenas que retornou um BadRequest
    }
    
    [Fact]
    public async Task GetEnrollment_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var enrollmentDto = new EnrollmentDto
        {
            Id = enrollmentId,
            StudentId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            EnrollmentDate = DateTime.UtcNow,
            Status = "Active"
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollmentDto);
        
        // Act
        var result = await _controller.GetEnrollment(enrollmentId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEnrollment = Assert.IsType<EnrollmentDto>(okResult.Value);
        Assert.Equal(enrollmentId, returnedEnrollment.Id);
    }
    
    [Fact]
    public async Task GetEnrollment_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Enrollment", enrollmentId));
        
        // Act
        var result = await _controller.GetEnrollment(enrollmentId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(enrollmentId.ToString(), notFoundResult.Value.ToString());
    }
    
    [Fact]
    public async Task GetStudentEnrollments_WithValidStudentId_ReturnsOkResult()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var enrollments = new List<EnrollmentDto>
        {
            new EnrollmentDto
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                CourseId = Guid.NewGuid(),
                EnrollmentDate = DateTime.UtcNow.AddDays(-30),
                Status = "Active"
            },
            new EnrollmentDto
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                CourseId = Guid.NewGuid(),
                EnrollmentDate = DateTime.UtcNow.AddDays(-60),
                Status = "Completed"
            }
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentEnrollmentsQuery>(q => q.StudentId == studentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);
        
        // Act
        var result = await _controller.GetStudentEnrollments(studentId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEnrollments = Assert.IsAssignableFrom<IEnumerable<EnrollmentDto>>(okResult.Value);
        Assert.Equal(2, returnedEnrollments.Count());
        Assert.All(returnedEnrollments, e => Assert.Equal(studentId, e.StudentId));
    }
    
    [Fact]
    public async Task GetStudentEnrollments_WithNonExistentStudent_ReturnsNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentEnrollmentsQuery>(q => q.StudentId == studentId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Student", studentId));
        
        // Act
        var result = await _controller.GetStudentEnrollments(studentId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(studentId.ToString(), notFoundResult.Value.ToString());
    }
    
    [Fact]
    public async Task GetStudentEnrollments_WithUnexpectedError_ReturnsBadRequest()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var errorMessage = "Database connection error";
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetStudentEnrollmentsQuery>(q => q.StudentId == studentId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));
        
        // Act
        var result = await _controller.GetStudentEnrollments(studentId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains(errorMessage, badRequestResult.Value.ToString());
    }
    
    [Fact]
    public async Task CompleteLesson_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        var commandResult = new CompleteLessonResult 
        { 
            IsCompleted = true,
            EnrollmentId = enrollmentId,
            LessonId = lessonId,
            Message = "Lesson completed successfully" 
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<CompleteLessonCommand>(cmd => 
                cmd.EnrollmentId == enrollmentId && 
                cmd.LessonId == lessonId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(commandResult);
        
        // Act
        var result = await _controller.CompleteLesson(enrollmentId, lessonId, request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<CompleteLessonResult>(okResult.Value);
        Assert.True(returnedResult.IsCompleted);
        Assert.Equal("Lesson completed successfully", returnedResult.Message);
    }
    
    [Fact]
    public async Task CompleteLesson_WithNonExistentEnrollment_ReturnsNotFoundResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<CompleteLessonCommand>(cmd => 
                cmd.EnrollmentId == enrollmentId && 
                cmd.LessonId == lessonId), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Enrollment", enrollmentId));
        
        // Act
        var result = await _controller.CompleteLesson(enrollmentId, lessonId, request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(enrollmentId.ToString(), notFoundResult.Value.ToString());
    }
    
    [Fact]
    public async Task CompleteLesson_WithValidationError_ReturnsBadRequestResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new LessonCompleteRequest { Completed = true };
        
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("EnrollmentId", "Invalid enrollment ID")
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<CompleteLessonCommand>(cmd => 
                cmd.EnrollmentId == enrollmentId && 
                cmd.LessonId == lessonId), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationFailures));
        
        // Act
        var result = await _controller.CompleteLesson(enrollmentId, lessonId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
    
    [Fact]
    public async Task CompleteCourse_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        // Mock GetEnrollmentByIdQuery result
        var enrollment = new EnrollmentDto
        {
            Id = enrollmentId,
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active.ToString()
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        
        // Create LearningHistory in the in-memory database
        var learningHistory = new LearningHistory(studentId);
        _dbContext.LearningHistories.Add(learningHistory);
        
        // Create CourseProgress with all lessons completed
        var courseProgress = new CourseProgress(courseId)
        {
            LearningHistoryId = studentId
        };
        
        // Mock GetCourseByIdQuery result
        var lessons = new List<LessonDto>
        {
            new LessonDto { Id = Guid.NewGuid(), Title = "Lesson 1" },
            new LessonDto { Id = Guid.NewGuid(), Title = "Lesson 2" }
        };
        
        var course = new CourseDto
        {
            Id = courseId,
            Lessons = lessons
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        
        // Add completed lessons to course progress
        foreach (var lesson in course.Lessons)
        {
            courseProgress.AddCompletedLesson(lesson.Id);
        }
        
        _dbContext.CourseProgresses.Add(courseProgress);
        await _dbContext.SaveChangesAsync();
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<CompleteEnrollmentCommand>(cmd => cmd.EnrollmentId == enrollmentId), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.CompleteCourse(enrollmentId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Course completed successfully", okResult.Value);
    }
    
    [Fact]
    public async Task CompleteCourse_WithNonExistentEnrollment_ReturnsNotFoundResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Enrollment", enrollmentId));
        
        // Act
        var result = await _controller.CompleteCourse(enrollmentId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(enrollmentId.ToString(), notFoundResult.Value.ToString());
    }
    
    [Fact]
    public async Task CompleteCourse_WithInactiveEnrollment_ReturnsBadRequestResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        // Mock GetEnrollmentByIdQuery result with Completed status
        var enrollment = new EnrollmentDto
        {
            Id = enrollmentId,
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Completed.ToString()
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        
        // Act
        var result = await _controller.CompleteCourse(enrollmentId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Only active enrollments can be completed.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task CompleteCourse_WithNotAllLessonsCompleted_ReturnsBadRequestResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        // Mock GetEnrollmentByIdQuery result
        var enrollment = new EnrollmentDto
        {
            Id = enrollmentId,
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active.ToString()
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        
        // Create LearningHistory in the in-memory database
        var learningHistory = new LearningHistory(studentId);
        _dbContext.LearningHistories.Add(learningHistory);
        
        // Create CourseProgress with NOT all lessons completed
        var courseProgress = new CourseProgress(courseId)
        {
            LearningHistoryId = studentId
        };
        
        // Mock GetCourseByIdQuery result
        var lesson1Id = Guid.NewGuid();
        var lesson2Id = Guid.NewGuid();
        
        var lessons = new List<LessonDto>
        {
            new LessonDto { Id = lesson1Id, Title = "Lesson 1" },
            new LessonDto { Id = lesson2Id, Title = "Lesson 2" }
        };
        
        var course = new CourseDto
        {
            Id = courseId,
            Lessons = lessons
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        
        // Add only one completed lesson to course progress
        courseProgress.AddCompletedLesson(lesson1Id);
        // Lesson2 is not completed
        
        _dbContext.CourseProgresses.Add(courseProgress);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _controller.CompleteCourse(enrollmentId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("All classes must be completed", badRequestResult.Value.ToString());
        Assert.Contains("1/2", badRequestResult.Value.ToString());
    }
    
    [Fact]
    public async Task CompleteCourse_WithUnexpectedError_ReturnsBadRequestResult()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        // Mock GetEnrollmentByIdQuery result
        var enrollment = new EnrollmentDto
        {
            Id = enrollmentId,
            StudentId = studentId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active.ToString()
        };
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnrollmentByIdQuery>(q => q.Id == enrollmentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        
        // Mock error during command execution
        var exceptionMessage = "Erro inesperado durante a conclusão do curso";
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetCourseByIdQuery>(q => q.CourseId == courseId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));
        
        // Act
        var result = await _controller.CompleteCourse(enrollmentId);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains(exceptionMessage, badRequestResult.Value.ToString());
    }
} 