using FluencyHub.StudentManagement.Application.Queries.GetStudentEnrollments;
using FluencyHub.StudentManagement.Application.Common.Interfaces;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.SharedKernel.Common.Exceptions;
using FluencyHub.Tests.Helpers;
using Moq;
using Xunit;
using FluentAssertions;
using IStudentRepository = FluencyHub.StudentManagement.Application.Common.Interfaces.IStudentRepository;

namespace FluencyHub.Tests.Unit.StudentManagement.Application.Queries;

public class GetStudentEnrollmentsQueryHandlerTests
{
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<IEnrollmentRepository> _mockEnrollmentRepository;
    private readonly GetStudentEnrollmentsQueryHandler _handler;

    public GetStudentEnrollmentsQueryHandlerTests()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();
        _handler = new GetStudentEnrollmentsQueryHandler(
            _mockStudentRepository.Object,
            _mockEnrollmentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEnrollments_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();
        var query = new GetStudentEnrollmentsQuery(studentId);

        var student = TestDataBuilder.CreateValidStudent();
        TestDataBuilder.SetEntityId(student, studentId);
        var enrollment1 = TestDataBuilder.CreateValidEnrollmentWithNavigation(studentId, courseId1, 299.99m);
        var enrollment2 = TestDataBuilder.CreateValidEnrollmentWithNavigation(studentId, courseId2, 399.99m);
        
        TestDataBuilder.SetEntityId(enrollment1, Guid.NewGuid());
        TestDataBuilder.SetEntityId(enrollment2, Guid.NewGuid());

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockStudentRepository
            .Setup(x => x.GetEnrollmentsByStudentIdAsync(studentId))
            .ReturnsAsync(new List<Enrollment> { enrollment1, enrollment2 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var enrollmentList = result.ToList();
        enrollmentList[0].StudentId.Should().Be(studentId);
        enrollmentList[0].CourseId.Should().Be(courseId1);
        enrollmentList[1].StudentId.Should().Be(studentId);
        enrollmentList[1].CourseId.Should().Be(courseId2);

        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
        _mockStudentRepository.Verify(x => x.GetEnrollmentsByStudentIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenStudentNotFound()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentEnrollmentsQuery(studentId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);
        
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Estudante com ID {studentId} não encontrado");
        
        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
        _mockEnrollmentRepository.Verify(x => x.GetByStudentIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenStudentHasNoEnrollments()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var query = new GetStudentEnrollmentsQuery(studentId);

        var student = TestDataBuilder.CreateValidStudent();
        TestDataBuilder.SetEntityId(student, studentId);
        var enrollments = new List<Enrollment>();

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockStudentRepository
            .Setup(x => x.GetEnrollmentsByStudentIdAsync(studentId))
            .ReturnsAsync(enrollments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockStudentRepository.Verify(x => x.GetByIdAsync(studentId), Times.Once);
        _mockStudentRepository.Verify(x => x.GetEnrollmentsByStudentIdAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEnrollmentsWithCorrectProperties_WhenStudentExists()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var query = new GetStudentEnrollmentsQuery(studentId);

        var student = TestDataBuilder.CreateValidStudent();
        TestDataBuilder.SetEntityId(student, studentId);
        var enrollment = TestDataBuilder.CreateValidEnrollmentWithNavigation(studentId, courseId, 299.99m);
        var enrollments = new List<Enrollment> { enrollment };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId))
            .ReturnsAsync(student);

        _mockStudentRepository
            .Setup(x => x.GetEnrollmentsByStudentIdAsync(studentId))
            .ReturnsAsync(enrollments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var enrollmentDto = result.First();
        enrollmentDto.Id.Should().Be(enrollment.Id);
        enrollmentDto.StudentId.Should().Be(studentId);
        enrollmentDto.CourseId.Should().Be(courseId);
        enrollmentDto.Price.Should().Be(299.99m);
        enrollmentDto.Status.Should().Be(enrollment.Status.ToString());
    }
} 