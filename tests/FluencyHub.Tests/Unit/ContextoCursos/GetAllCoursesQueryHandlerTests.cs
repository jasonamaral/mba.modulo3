using FluencyHub.ContentManagement.Application.Common.Models;
using FluencyHub.ContentManagement.Application.Queries.GetAllCourses;
using FluencyHub.ContentManagement.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FluencyHub.Tests.Unit.ContextoCursos;

public class GetAllCoursesQueryHandlerTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly GetAllCoursesQueryHandler _handler;

    public GetAllCoursesQueryHandlerTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _handler = new GetAllCoursesQueryHandler(_courseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCourses_WhenCoursesExist()
    {
        // Arrange
        var query = new GetAllCoursesQuery();
        var courses = new List<Course>
        {
            CreateCourse("Course 1", "Description 1", 99.99m),
            CreateCourse("Course 2", "Description 2", 149.99m)
        };

        _courseRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(courses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        var coursesList = result.ToList();
        Assert.Equal("Course 1", coursesList[0].Name);
        Assert.Equal("Description 1", coursesList[0].Description);
        Assert.Equal(99.99m, coursesList[0].Price);
        Assert.Equal("Course 2", coursesList[1].Name);
        Assert.Equal("Description 2", coursesList[1].Description);
        Assert.Equal(149.99m, coursesList[1].Price);

        _courseRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCoursesExist()
    {
        // Arrange
        var query = new GetAllCoursesQuery();
        var courses = new List<Course>();

        _courseRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(courses);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _courseRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    private Course CreateCourse(string name, string description, decimal price)
    {
        var courseContent = new CourseContent(
            "Syllabus",
            "Learning Objectives",
            "Pre-requisites",
            "Target Audience",
            "English",
            "Beginner"
        );
        
        return new Course(name, description, courseContent, price);
    }
} 