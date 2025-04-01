using FluencyHub.API.Controllers;
using FluencyHub.Application.Common.Exceptions;
using FluencyHub.Application.ContentManagement.Commands.CreateCourse;
using FluencyHub.Application.ContentManagement.Queries.GetCourseById;
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
        var exceptionMessage = $"Entity \"Course\" ({courseId}) was not found.";
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCourseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Course", courseId));
        
        // Act
        var result = await _controller.GetCourseById(courseId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(exceptionMessage, notFoundResult.Value);
    }
    
    [Fact]
    public async Task CreateCourse_WithValidCommand_ReturnsCreatedAtAction()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new CreateCourseCommand
        {
            Name = "Test Course",
            Description = "Test Description",
            Syllabus = "Test Syllabus",
            LearningObjectives = "Test Objectives",
            PreRequisites = "Test Prerequisites",
            TargetAudience = "Test Audience",
            Language = "English",
            Level = "Beginner",
            Price = 99.99m
        };
        
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseId);
        
        // Act
        var result = await _controller.CreateCourse(command);
        
        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetCourseById", createdAtActionResult.ActionName);
        Assert.Equal(courseId, createdAtActionResult.RouteValues["id"]);
        Assert.Null(createdAtActionResult.Value);
    }
    
    [Fact]
    public async Task CreateCourse_WithInvalidCommand_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "", // Empty name
            Description = "Test Description",
            Syllabus = "Test Syllabus",
            LearningObjectives = "Test Objectives",
            PreRequisites = "Test Prerequisites",
            TargetAudience = "Test Audience",
            Language = "English",
            Level = "Beginner",
            Price = 99.99m
        };
        
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException());
        
        // Act
        var result = await _controller.CreateCourse(command);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
} 