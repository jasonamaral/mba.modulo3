using Xunit;
using Moq;
using FluentAssertions;
using FluencyHub.StudentManagement.Application.Queries.GetAllStudents;
using FluencyHub.StudentManagement.Application.Common.Models;
using FluencyHub.StudentManagement.Domain;
using AppInterfaces = FluencyHub.StudentManagement.Application.Common.Interfaces;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetAllStudentsQueryHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly GetAllStudentsQueryHandler _handler;

    public GetAllStudentsQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _handler = new GetAllStudentsQueryHandler(_mockStudentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllStudents_WhenStudentsExist()
    {
        // Arrange
        var students = new List<Student>
        {
            new("João", "Silva", "joao@email.com", new DateTime(1990, 1, 1)),
            new("Maria", "Santos", "maria@email.com", new DateTime(1985, 5, 15))
        };

        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().FirstName.Should().Be("João");
        result.Last().FirstName.Should().Be("Maria");
        
        _mockStudentRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoStudentsExist()
    {
        // Arrange
        var students = new List<Student>();
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        
        _mockStudentRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnStudentsWithBasicInfo()
    {
        // Arrange
        var student = new Student("Ana", "Costa", "ana@email.com", new DateTime(1992, 3, 20));

        var students = new List<Student> { student };
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var studentDto = result.First();
        studentDto.FirstName.Should().Be("Ana");
        studentDto.LastName.Should().Be("Costa");
        studentDto.Email.Should().Be("ana@email.com");
        studentDto.IsActive.Should().BeTrue();
        
        _mockStudentRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleRepositoryException()
    {
        // Arrange
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Erro no banco de dados"));

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro no banco de dados");
    }
} 