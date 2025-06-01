using FluencyHub.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluencyHub.StudentManagement.Application.Commands.CreateStudent;
using FluencyHub.StudentManagement.Application.Commands.UpdateStudent;
using FluencyHub.StudentManagement.Application.Commands.ActivateStudent;
using FluencyHub.StudentManagement.Application.Commands.DeactivateStudent;
using FluencyHub.StudentManagement.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Domain;
using FluencyHub.StudentManagement.Application.Queries.GetStudentProgress;

namespace FluencyHub.Tests.Integration.Controllers;

public class EstudantesControllerTests : IntegrationTestBase
{
    // 182. EstudantesController_GetAllStudents_ShouldReturnAllStudents_WhenAdminRole
    [Fact]
    public async Task GetAllStudents_ShouldReturnAllStudents_WhenAdminRole()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        await SeedStudentsAsync();

        // Act
        var response = await adminClient.GetAsync("/api/estudantes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var students = await response.Content.ReadFromJsonAsync<IEnumerable<StudentDto>>();
        students.Should().NotBeEmpty();
    }

    // 183. EstudantesController_GetAllStudents_ShouldReturnForbidden_WhenStudentRole
    [Fact]
    public async Task GetAllStudents_ShouldReturnForbidden_WhenStudentRole()
    {
        // Arrange
        var studentClient = CreateAuthenticatedClient("student@example.com", "Student");

        // Act
        var response = await studentClient.GetAsync("/api/estudantes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // 184. EstudantesController_GetCurrentStudent_ShouldReturnCurrentStudent_WhenAuthenticated
    [Fact]
    public async Task GetCurrentStudent_ShouldReturnCurrentStudent_WhenAuthenticated()
    {
        // Arrange
        var email = "current@example.com";
        var client = CreateAuthenticatedClient(email);
        await SeedStudentAsync(email);

        // Act
        var response = await client.GetAsync("/api/estudantes/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        student.Should().NotBeNull();
        student!.Email.Should().Be(email);
    }

    // 185. EstudantesController_GetStudentById_ShouldReturnStudent_WhenStudentExists
    [Fact]
    public async Task GetStudentById_ShouldReturnStudent_WhenStudentExists()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var studentId = await SeedStudentAsync();

        // Act
        var response = await adminClient.GetAsync($"/api/estudantes/{studentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        student.Should().NotBeNull();
        student!.Id.Should().Be(studentId);
    }

    // 186. EstudantesController_CreateStudent_ShouldCreateStudent_WhenValidRequest
    [Fact]
    public async Task CreateStudent_ShouldCreateStudent_WhenValidRequest()
    {
        // Arrange
        var command = new CreateStudentCommand
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao.silva@email.com",
            Password = "MinhaSenh@123",
            PhoneNumber = "+5511999999999",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "Rua das Flores, 123",
            City = "São Paulo",
            State = "SP",
            Country = "Brasil",
            PostalCode = "01234-567"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/estudantes", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    // 190. EstudantesController_GetStudentProgress_ShouldReturnProgress_WhenStudentExists
    [Fact]
    public async Task GetStudentProgress_ShouldReturnProgress_WhenStudentExists()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var studentId = await SeedStudentAsync();

        // Act
        var response = await adminClient.GetAsync($"/api/estudantes/{studentId}/progresso");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var progress = await response.Content.ReadFromJsonAsync<StudentProgressViewModel>();
        progress.Should().NotBeNull();
    }

    private async Task<Guid> SeedStudentAsync(string email = "test@example.com")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        
        var student = TestDataBuilder.CreateValidStudent(email: email);
        TestDataBuilder.SetEntityId(student, Guid.NewGuid());
        
        context.Students.Add(student);
        await context.SaveChangesAsync();
        
        return student.Id;
    }

    private async Task SeedStudentsAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        
        var students = new[]
        {
            TestDataBuilder.CreateValidStudent(firstName: "João", lastName: "Silva", email: "joao@example.com"),
            TestDataBuilder.CreateValidStudent(firstName: "Maria", lastName: "Santos", email: "maria@example.com"),
            TestDataBuilder.CreateValidStudent(firstName: "Pedro", lastName: "Oliveira", email: "pedro@example.com")
        };

        foreach (var student in students)
        {
            TestDataBuilder.SetEntityId(student, Guid.NewGuid());
            context.Students.Add(student);
        }
        
        await context.SaveChangesAsync();
    }
} 