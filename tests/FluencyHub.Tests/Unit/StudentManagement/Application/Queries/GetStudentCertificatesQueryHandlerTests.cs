using FluencyHub.StudentManagement.Application.Queries.GetStudentCertificates;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using FluencyHub.Tests.Helpers;
using Moq;
using Xunit;
using FluentAssertions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetStudentCertificatesQueryHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<ICertificateRepository> _mockCertificateRepository;
    private readonly GetStudentCertificatesQueryHandler _handler;

    public GetStudentCertificatesQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockCertificateRepository = new Mock<ICertificateRepository>();
        _handler = new GetStudentCertificatesQueryHandler(
            _mockStudentRepository.Object,
            _mockCertificateRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCertificates_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();
        var query = new GetStudentCertificatesQuery(studentId);

        var student = TestDataBuilder.CreateValidStudent();
        TestDataBuilder.SetEntityId(student, studentId);
        var certificate1 = TestDataBuilder.CreateValidCertificate(studentId, courseId1, "Curso de Inglês");
        var certificate2 = TestDataBuilder.CreateValidCertificate(studentId, courseId2, "Curso de Espanhol");
        
        TestDataBuilder.SetEntityId(certificate1, Guid.NewGuid());
        TestDataBuilder.SetEntityId(certificate2, Guid.NewGuid());

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockCertificateRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Certificate> { certificate1, certificate2 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var certificateList = result.ToList();
        certificateList[0].StudentId.Should().Be(studentId);
        certificateList[0].CourseId.Should().Be(courseId1);
        certificateList[1].StudentId.Should().Be(studentId);
        certificateList[1].CourseId.Should().Be(courseId2);

        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
        _mockCertificateRepository.Verify(x => x.GetByStudentIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenStudentNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentCertificatesQuery(studentId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Estudante com ID {studentId} não encontrado");
        
        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
        _mockCertificateRepository.Verify(x => x.GetByStudentIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenStudentHasNoCertificates()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentCertificatesQuery(studentId);

        var student = TestDataBuilder.CreateValidStudent();
        TestDataBuilder.SetEntityId(student, studentId);
        var certificates = new List<Certificate>();

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockCertificateRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(certificates);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
        _mockCertificateRepository.Verify(x => x.GetByStudentIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCertificatesWithCorrectProperties_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetStudentCertificatesQuery(studentId);

        var student = TestDataBuilder.CreateValidStudent();
        TestDataBuilder.SetEntityId(student, studentId);
        var certificate = TestDataBuilder.CreateValidCertificate(studentId, courseId, "Certificado de Inglês Básico");
        certificate.SetScore(85);
        certificate.SetFeedback("Excelente desempenho!");
        
        var certificates = new List<Certificate> { certificate };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockCertificateRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(certificates);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var certificateDto = result.First();
        certificateDto.Id.Should().Be(certificate.Id);
        certificateDto.StudentId.Should().Be(studentId);
        certificateDto.CourseId.Should().Be(courseId);
        certificateDto.StudentName.Should().Be($"{student.FirstName} {student.LastName}");
        certificateDto.CertificateNumber.Should().Be(certificate.CertificateNumber);
        certificateDto.Score.Should().Be(85);
        certificateDto.Feedback.Should().Be("Excelente desempenho!");
    }

    [Fact]
    public async Task Handle_ShouldReturnCertificatesWithStudentName_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetStudentCertificatesQuery(studentId);

        var student = TestDataBuilder.CreateValidStudent(firstName: "Maria", lastName: "Santos");
        TestDataBuilder.SetEntityId(student, studentId);
        var certificate = TestDataBuilder.CreateValidCertificate(studentId, courseId, "Certificado de Inglês");
        var certificates = new List<Certificate> { certificate };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockCertificateRepository
            .Setup(x => x.GetByStudentIdAsync(studentId))
            .ReturnsAsync(certificates);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var certificateDto = result.First();
        certificateDto.StudentName.Should().Be("Maria Santos");
    }
} 