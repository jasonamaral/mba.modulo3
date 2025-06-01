using FluencyHub.StudentManagement.Application.Queries.GetStudentByEmail;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using Moq;
using Xunit;
using FluentAssertions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetStudentByEmailQueryHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly GetStudentByEmailQueryHandler _handler;

    public GetStudentByEmailQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _handler = new GetStudentByEmailQueryHandler(_mockStudentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnStudent_WhenStudentExists()
    {
        // Arrange
        var email = "joao@email.com";
        var student = new Student("João", "Silva", email, new DateTime(1990, 1, 1));

        var query = new GetStudentByEmailQuery(email);

        _mockStudentRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(student);

        _mockStudentRepository
            .Setup(x => x.GetEnrollmentsByStudentIdAsync(student.Id))
            .ReturnsAsync(new List<Enrollment>());

        _mockStudentRepository
            .Setup(x => x.GetCertificatesByStudentIdAsync(student.Id))
            .ReturnsAsync(new List<Certificate>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(student.Id);
        result.FirstName.Should().Be(student.FirstName);
        result.LastName.Should().Be(student.LastName);
        result.Email.Should().Be(student.Email);
        result.IsActive.Should().Be(student.IsActive);
        
        _mockStudentRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStudentNotFound()
    {
        // Arrange
        var email = "naoexiste@email.com";
        var query = new GetStudentByEmailQuery(email);

        _mockStudentRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Estudante com email {email} não encontrado");
        
        _mockStudentRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Theory]
    [InlineData("JOAO@EMAIL.COM")]
    [InlineData("joao@EMAIL.com")]
    [InlineData("Joao@Email.Com")]
    public async Task Handle_ShouldHandleEmailCaseInsensitive_WhenStudentExists(string emailVariation)
    {
        // Arrange
        var originalEmail = "joao@email.com";
        var student = new Student("João", "Silva", originalEmail, new DateTime(1990, 1, 1));

        var query = new GetStudentByEmailQuery(emailVariation);

        _mockStudentRepository
            .Setup(x => x.GetByEmailAsync(emailVariation))
            .ReturnsAsync(student);

        _mockStudentRepository
            .Setup(x => x.GetEnrollmentsByStudentIdAsync(student.Id))
            .ReturnsAsync(new List<Enrollment>());

        _mockStudentRepository
            .Setup(x => x.GetCertificatesByStudentIdAsync(student.Id))
            .ReturnsAsync(new List<Certificate>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(originalEmail);
        
        _mockStudentRepository.Verify(x => x.GetByEmailAsync(emailVariation), Times.Once);
    }
} 