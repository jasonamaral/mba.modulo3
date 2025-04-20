using FluencyHub.API.Controllers;
using FluencyHub.Application.Common.Exceptions;
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
} 