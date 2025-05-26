using FluentAssertions;
using FluencyHub.ContentManagement.Application.Queries.GetCourseById;
using FluencyHub.ContentManagement.Domain;
using FluencyHub.Tests.Helpers;
using Moq;
using Xunit;
using ICourseRepository = FluencyHub.ContentManagement.Application.Common.Interfaces.ICourseRepository;

namespace FluencyHub.Tests.Unit.ContentManagement.Application.Queries;

public class GetCourseByIdQueryHandlerTests
{
    private readonly Mock<ICourseRepository> _mockCourseRepository;
    private readonly GetCourseByIdQueryHandler _handler;

    public GetCourseByIdQueryHandlerTests()
    {
        _mockCourseRepository = new Mock<ICourseRepository>();
        _handler = new GetCourseByIdQueryHandler(_mockCourseRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidCourseId_ShouldReturnCourseDto()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = TestDataBuilder.CreateValidCourse();
        var query = new GetCourseByIdQuery { CourseId = courseId };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(course.Id);
        result.Name.Should().Be(course.Name);
        result.Description.Should().Be(course.Description);
        result.Syllabus.Should().Be(course.Content.Syllabus);
        result.LearningObjectives.Should().Be(course.Content.LearningObjectives);
        result.PreRequisites.Should().Be(course.Content.PreRequisites);
        result.TargetAudience.Should().Be(course.Content.TargetAudience);
        result.Language.Should().Be(course.Content.Language);
        result.Level.Should().Be(course.Content.Level);
        result.Price.Should().Be(course.Price);
        result.IsActive.Should().Be(course.IsActive);
        result.CreatedAt.Should().Be(course.CreatedAt);
        result.UpdatedAt.Should().Be(course.UpdatedAt);

        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCourseId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var query = new GetCourseByIdQuery { CourseId = courseId };

        _mockCourseRepository
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));

        exception.Message.Should().Contain($"Course with ID {courseId} not found");
        _mockCourseRepository.Verify(x => x.GetByIdAsync(courseId), Times.Once);
    }
} 