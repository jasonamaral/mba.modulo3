using FluencyHub.Application.Common.Interfaces;
using FluencyHub.ContentManagement.Application.Commands.CreateCourse;
using FluencyHub.ContentManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluencyHub.Tests.Application.ContentManagement.Commands;

public class CreateCourseCommandHandlerTests
{
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly Mock<ILogger<CreateCourseCommandHandler>> _loggerMock;
    private readonly CreateCourseCommandHandler _handler;
    
    public CreateCourseCommandHandlerTests()
    {
        _mockCourseRepository = new Mock<ICourseRepository>();
        _loggerMock = new Mock<ILogger<CreateCourseCommandHandler>>();
        _handler = new CreateCourseCommandHandler(_mockCourseRepository.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewCourseId()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Test Course",
            Description = "Test Description",
            Syllabus = "Test Syllabus",
            LearningObjectives = "Test Objectives",
            PreRequisites = "Test Prerequisites",
            TargetAudience = "Beginners",
            Language = "English",
            Level = "A1",
            Price = 99.99m
        };
        
        var validCourseId = Guid.NewGuid();
        Course savedCourse = null;
        
        _mockCourseRepository
            .Setup(r => r.AddAsync(It.IsAny<Course>()))
            .Callback<Course>(course => 
            {
                // Definir o Id diretamente na propriedade Id
                course.Id = validCourseId;
                savedCourse = course;
            })
            .ReturnsAsync((Course course) => course);
            
        _mockCourseRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.Equal(validCourseId, result);
        
        _mockCourseRepository.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Once);
        _mockCourseRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.NotNull(savedCourse);
        Assert.Equal(command.Name, savedCourse.Name);
        Assert.Equal(command.Description, savedCourse.Description);
        Assert.Equal(command.Price, savedCourse.Price);
        Assert.Equal(command.Syllabus, savedCourse.Content.Syllabus);
        Assert.Equal(command.Language, savedCourse.Content.Language);
        Assert.Equal(command.Level, savedCourse.Content.Level);
    }
    
    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var command = new CreateCourseCommand
        {
            Name = "Test Course",
            Description = "Test Description",
            Syllabus = "Test Syllabus",
            LearningObjectives = "Test Objectives",
            PreRequisites = "Test Prerequisites",
            TargetAudience = "Beginners",
            Language = "English",
            Level = "A1",
            Price = 99.99m
        };
        
        var expectedException = new Exception("Database error");
        
        _mockCourseRepository
            .Setup(r => r.AddAsync(It.IsAny<Course>()))
            .Throws(expectedException);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(command, CancellationToken.None));
            
        Assert.Same(expectedException, exception);
        
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.Is<Exception>(ex => ex == expectedException),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 