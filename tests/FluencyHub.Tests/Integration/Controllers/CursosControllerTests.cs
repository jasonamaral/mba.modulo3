using FluencyHub.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluencyHub.API.Models;
using FluencyHub.ContentManagement.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.ContentManagement.Domain;

namespace FluencyHub.Tests.Integration.Controllers;

public class CursosControllerTests : IntegrationTestBase
{
    // 191. CursosController_GetAllCourses_ShouldReturnAllCourses
    [Fact]
    public async Task GetAllCourses_ShouldReturnAllCourses()
    {
        // Arrange
        await SeedCoursesAsync();

        // Act
        var response = await Client.GetAsync("/api/cursos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var courses = await response.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        courses.Should().NotBeEmpty();
    }

    // 192. CursosController_GetCourseById_ShouldReturnCourse_WhenCourseExists
    [Fact]
    public async Task GetCourseById_ShouldReturnCourse_WhenCourseExists()
    {
        // Arrange
        var courseId = await SeedCourseAsync();

        // Act
        var response = await Client.GetAsync($"/api/cursos/{courseId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var course = await response.Content.ReadFromJsonAsync<CourseDto>();
        course.Should().NotBeNull();
        course!.Id.Should().Be(courseId);
    }

    // 193. CursosController_GetCourseById_ShouldReturnNotFound_WhenCourseNotExists
    [Fact]
    public async Task GetCourseById_ShouldReturnNotFound_WhenCourseNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/cursos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // 195. CursosController_CreateCourse_ShouldReturnForbidden_WhenStudentRole
    [Fact]
    public async Task CreateCourse_ShouldReturnForbidden_WhenStudentRole()
    {
        // Arrange
        var studentClient = CreateAuthenticatedClient("student@example.com", "Student");
        var request = new CourseCreateRequest
        {
            Name = "Curso Não Autorizado",
            Description = "Tentativa de criação por estudante",
            Price = 199.99m,
            Language = "Português",
            Level = "Básico"
        };

        // Act
        var response = await studentClient.PostAsJsonAsync("/api/cursos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // 196. CursosController_UpdateCourse_ShouldUpdateCourse_WhenAdminRole
    [Fact]
    public async Task UpdateCourse_ShouldUpdateCourse_WhenAdminRole()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var courseId = await SeedCourseAsync();
        
        var request = new CourseUpdateRequest
        {
            Id = courseId,
            Name = "Curso de Inglês Básico Atualizado",
            Description = "Descrição atualizada do curso",
            Language = "Português",
            Level = "Iniciante",
            Price = 349.99m,
            Syllabus = "Programa atualizado do curso",
            LearningObjectives = "Objetivos atualizados",
            PreRequisites = "Nenhum pré-requisito",
            TargetAudience = "Iniciantes"
        };

        // Act
        var response = await adminClient.PutAsJsonAsync($"/api/cursos/{courseId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<Guid> SeedCourseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        
        var course = TestDataBuilder.CreateValidCourse();
        TestDataBuilder.SetEntityId(course, Guid.NewGuid());
        
        context.Courses.Add(course);
        await context.SaveChangesAsync();
        
        return course.Id;
    }

    private async Task SeedCoursesAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        
        var courses = new[]
        {
            TestDataBuilder.CreateValidCourse(name: "Inglês Básico", description: "Curso de inglês para iniciantes"),
            TestDataBuilder.CreateValidCourse(name: "Espanhol Intermediário", description: "Curso de espanhol intermediário"),
            TestDataBuilder.CreateValidCourse(name: "Francês Avançado", description: "Curso de francês avançado")
        };

        foreach (var course in courses)
        {
            TestDataBuilder.SetEntityId(course, Guid.NewGuid());
            context.Courses.Add(course);
        }
        
        await context.SaveChangesAsync();
    }
} 