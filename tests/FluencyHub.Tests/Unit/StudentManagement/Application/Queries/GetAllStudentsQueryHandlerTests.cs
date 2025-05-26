using Xunit;
using Moq;
using FluentAssertions;
using FluencyHub.StudentManagement.Application.Queries.GetAllStudents;
using FluencyHub.StudentManagement.Domain;
using AppInterfaces = FluencyHub.StudentManagement.Application.Common.Interfaces;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetAllStudentsQueryHandlerTests
{
    private readonly Mock<AppInterfaces.IStudentRepository> _mockStudentRepository;
    private readonly GetAllStudentsQueryHandler _handler;

    public GetAllStudentsQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<AppInterfaces.IStudentRepository>();
        _handler = new GetAllStudentsQueryHandler(_mockStudentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveStudents_WhenStudentsExist()
    {
        // Arrange
        var students = new List<Student>
        {
            CreateValidStudent("John", "Doe", "john.doe@example.com"),
            CreateValidStudent("Jane", "Smith", "jane.smith@example.com")
        };
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var resultList = result.ToList();
        resultList[0].FirstName.Should().Be("John");
        resultList[0].LastName.Should().Be("Doe");
        resultList[0].Email.Should().Be("john.doe@example.com");
        resultList[1].FirstName.Should().Be("Jane");
        resultList[1].LastName.Should().Be("Smith");
        resultList[1].Email.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoStudentsExist()
    {
        // Arrange
        var students = new List<Student>();
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var students = new List<Student>();
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockStudentRepository.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapStudentPropertiesCorrectly()
    {
        // Arrange
        var student = CreateValidStudentWithCompleteInfo();
        var students = new List<Student> { student };
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(student.Id);
        dto.FirstName.Should().Be(student.FirstName);
        dto.LastName.Should().Be(student.LastName);
        dto.Email.Should().Be(student.Email);
        dto.PhoneNumber.Should().Be(student.PhoneNumber ?? string.Empty);
        dto.IsActive.Should().Be(student.IsActive);
        dto.CreatedAt.Should().Be(student.CreatedAt);
    }

    [Fact]
    public async Task Handle_ShouldHandleNullPhoneNumber()
    {
        // Arrange
        var student = CreateValidStudent("John", "Doe", "john.doe@example.com");
        var students = new List<Student> { student };
        var query = new GetAllStudentsQuery();

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.PhoneNumber.Should().Be(string.Empty);
    }

    private static Student CreateValidStudent(string firstName, string lastName, string email)
    {
        return new Student(firstName, lastName, email, new DateTime(1990, 1, 1));
    }

    private static Student CreateValidStudentWithCompleteInfo()
    {
        var student = new Student("John", "Doe", "john.doe@example.com", new DateTime(1990, 1, 1));
        
        // Usar reflection para definir propriedades privadas para teste
        var phoneProperty = typeof(Student).GetProperty("PhoneNumber");
        phoneProperty?.SetValue(student, "123-456-7890");
        
        return student;
    }
} 