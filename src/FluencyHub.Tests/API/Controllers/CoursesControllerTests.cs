using FluencyHub.API.Controllers;
using FluencyHub.API.Models;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Commands.CreateCourse;
using FluencyHub.Application.ContentManagement.Commands.UpdateCourse;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
using FluencyHub.Domain.ContentManagement;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FluencyHub.Tests.API.Controllers;

public class CoursesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CoursesController _controller;
    
    public CoursesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CoursesController(_mediatorMock.Object);
    }
    
    [Fact]
    public async Task GetCourseById_WithValidId_ReturnsOkWithCourse()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var courseDto = new CourseDto
        {
            Id = courseId,
            Name = "Test Course",
            Description = "Test Description",
            Content = new CourseContentDto
            {
                Syllabus = "Test Syllabus",
                LearningObjectives = "Test Objectives",
                PreRequisites = "Test Prerequisites",
                TargetAudience = "Test Audience",
                Language = "English",
                Level = "Beginner"
            },
            Price = 99.99m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCourseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDto);
        
        // Act
        var result = await _controller.GetCourseById(courseId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCourse = Assert.IsType<CourseDto>(okResult.Value);
        Assert.Equal(courseId, returnedCourse.Id);
        Assert.Equal("Test Course", returnedCourse.Name);
        Assert.Equal("Test Description", returnedCourse.Description);
    }
    
    [Fact]
    public async Task GetCourseById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCourseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Course), courseId));
        
        // Act
        var result = await _controller.GetCourseById(courseId);
        
        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
    
    [Fact]
    public async Task CreateCourse_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new CourseCreateRequest
        {
            Name = "Test Course",
            Description = "Test Description",
            Price = 99.99m,
            Content = new CourseContentRequest 
            {
                Description = "Test Syllabus",
                Goals = "Test Objectives",
                Requirements = "Test Prerequisites"
            }
        };
        
        var command = request.ToCommand();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseId);
        
        // Act
        var result = await _controller.CreateCourse(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetCourseById), createdResult.ActionName);
        Assert.Equal(courseId, createdResult.RouteValues["id"]);
    }
    
    [Fact]
    public async Task CreateCourse_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new CourseCreateRequest
        {
            Name = "", // Nome inválido
            Description = "Test Description",
            Price = 99.99m,
            Content = new CourseContentRequest 
            {
                Description = "Test Syllabus",
                Goals = "Test Objectives",
                Requirements = "Test Prerequisites"
            }
        };
        
        var command = request.ToCommand();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateCourseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException());
        
        // Act
        var result = await _controller.CreateCourse(request);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
    [Fact]
    public async Task UpdateCourse_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new CourseUpdateRequest
        {
            Id = courseId,
            Name = "Updated Course",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Advanced",
            Language = "Portuguese",
            Level = "C1",
            Price = 149.99m
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCourseCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await _controller.UpdateCourse(courseId, request);
        
        // Assert
        Assert.IsType<OkResult>(result);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateCourseCommand>(c => 
                c.Id == courseId && 
                c.Name == request.Name && 
                c.Description == request.Description &&
                c.Price == request.Price), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateCourse_WithMismatchingIds_ReturnsBadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var request = new CourseUpdateRequest
        {
            Id = differentId,
            Name = "Updated Course",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Advanced",
            Language = "Portuguese",
            Level = "C1",
            Price = 149.99m
        };
        
        // Act
        var result = await _controller.UpdateCourse(courseId, request);
        
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("ID", badRequestResult.Value.ToString());
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateCourseCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateCourse_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new CourseUpdateRequest
        {
            Id = courseId,
            Name = "Updated Course",
            Description = "Updated Description",
            Syllabus = "Updated Syllabus",
            LearningObjectives = "Updated Objectives",
            PreRequisites = "Updated Prerequisites",
            TargetAudience = "Advanced",
            Language = "Portuguese",
            Level = "C1",
            Price = 149.99m
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCourseCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Curso com ID {courseId} não encontrado"));
        
        // Act
        var result = await _controller.UpdateCourse(courseId, request);
        
        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
} 