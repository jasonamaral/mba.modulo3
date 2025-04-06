using FluencyHub.API.Controllers;
using FluencyHub.API.Models;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.StudentManagement.Commands.EnrollStudent;
using FluencyHub.Application.StudentManagement.Queries.GetEnrollmentById;
using FluencyHub.Application.StudentManagement.Queries.GetStudentEnrollments;
using FluencyHub.Domain.StudentManagement;
using FluencyHub.Infrastructure.Persistence;
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
} 