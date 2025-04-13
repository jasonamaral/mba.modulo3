using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Application.StudentManagement.Queries.GetAllStudents;
using FluencyHub.Domain.StudentManagement;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluencyHub.Tests.Application.StudentManagement.Queries;

public class GetAllStudentsQueryHandlerTests
{
    private readonly Mock<FluencyHub.Application.Common.Interfaces.IStudentRepository> _studentRepositoryMock;
    private readonly Mock<ILogger<GetAllStudentsQueryHandler>> _loggerMock;
    private readonly GetAllStudentsQueryHandler _handler;
    
    public GetAllStudentsQueryHandlerTests()
    {
        _studentRepositoryMock = new Mock<FluencyHub.Application.Common.Interfaces.IStudentRepository>();
        _loggerMock = new Mock<ILogger<GetAllStudentsQueryHandler>>();
        _handler = new GetAllStudentsQueryHandler(_studentRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ReturnsAllStudents()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student("Student 1", "Last 1", "student1@example.com", DateTime.Now, "+1234567890"),
            new Student("Student 2", "Last 2", "student2@example.com", DateTime.Now, "+1234567891"),
            new Student("Student 3", "Last 3", "student3@example.com", DateTime.Now, "+1234567892")
        };
        
        _studentRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(students);
        
        // Act
        var result = await _handler.Handle(new GetAllStudentsQuery(), CancellationToken.None);
        
        // Assert
        var studentDtos = result.ToList();
        Assert.Equal(3, studentDtos.Count);
        
        Assert.Equal("Student 1 Last 1", studentDtos[0].Name);
        Assert.Equal("student1@example.com", studentDtos[0].Email);
        Assert.Equal("+1234567890", studentDtos[0].PhoneNumber);
        
        Assert.Equal("Student 2 Last 2", studentDtos[1].Name);
        Assert.Equal("student2@example.com", studentDtos[1].Email);
        Assert.Equal("+1234567891", studentDtos[1].PhoneNumber);
        
        Assert.Equal("Student 3 Last 3", studentDtos[2].Name);
        Assert.Equal("student3@example.com", studentDtos[2].Email);
        Assert.Equal("+1234567892", studentDtos[2].PhoneNumber);
    }
    
    [Fact]
    public async Task Handle_EmptyList_ReturnsEmptyResult()
    {
        // Arrange
        _studentRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Student>());
        
        // Act
        var result = await _handler.Handle(new GetAllStudentsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Database error");
        
        _studentRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(expectedException);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(new GetAllStudentsQuery(), CancellationToken.None));
            
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