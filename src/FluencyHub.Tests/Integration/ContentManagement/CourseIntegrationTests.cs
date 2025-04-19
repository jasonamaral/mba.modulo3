using System.Net;
using System.Net.Http.Json;
using FluencyHub.Tests.Integration.Helpers;
using System.Text.Json;
using FluencyHub.Domain.ContentManagement;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.Infrastructure.Persistence;

namespace FluencyHub.Tests.Integration.ContentManagement;

[Trait("Category", "Integration")]
public class CourseIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public CourseIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task CreateCourse_WhenAdmin_ShouldCreateCourseSuccessfully()
    {
        // Arrange
        var token = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, token);

        var courseRequest = new
        {
            Name = "English for Beginners",
            Description = "Basic English course for beginners",
            Price = 199.90M,
            Content = new
            {
                Description = "In this course you will learn the basics of English",
                Goals = "Speak basic English in everyday situations",
                Requirements = "No prior knowledge required"
            }
        };

        // Act
        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);

        // Assert
        courseResponse.EnsureSuccessStatusCode();
        var content = await courseResponse.Content.ReadAsStringAsync();
        
        // Diagnóstico para entender o formato exato da resposta
        Console.WriteLine($"Resposta JSON: {content}");
        
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        
        // Vamos iterar por todas as propriedades para ver o que temos
        foreach (var property in root.EnumerateObject())
        {
            Console.WriteLine($"Propriedade: {property.Name}, Valor: {property.Value}");
        }
        
        // Obter o ID diretamente como Guid
        var courseId = root.GetProperty("id").GetGuid().ToString();
        
        Assert.NotNull(courseId);
        
        // Agora tentamos obter o curso criado para validar
        var getResponse = await _client.GetAsync($"/api/courses/{courseId}");
        getResponse.EnsureSuccessStatusCode();

        var getCourseContent = await getResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(getCourseContent);
        var courseRoot = courseDocument.RootElement;
        
        Assert.Equal("English for Beginners", courseRoot.GetProperty("name").GetString());
        Assert.Equal("Basic English course for beginners", courseRoot.GetProperty("description").GetString());

        // Agora tenta adicionar uma aula ao curso
        var lessonRequest = new
        {
            Title = "Introduction and Greetings",
            Content = "In this lesson you will learn the most common greetings in English such as: Hello, Hi, Good morning, etc.",
            Order = 1
        };

        var lessonResponse = await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lessonRequest);
        lessonResponse.EnsureSuccessStatusCode();
        
        var lessonContent = await lessonResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Lesson Response: {lessonContent}");

        // Obter o curso para verificar se a aula foi adicionada
        getResponse = await _client.GetAsync($"/api/courses/{courseId}");
        getResponse.EnsureSuccessStatusCode();

        getCourseContent = await getResponse.Content.ReadAsStringAsync();
        using var updatedCourseDocument = JsonDocument.Parse(getCourseContent);
        var updatedCourseRoot = updatedCourseDocument.RootElement;
        
        // Verificar se o curso tem a aula adicionada
        var lessons = updatedCourseRoot.GetProperty("lessons");
        Assert.Equal(1, lessons.GetArrayLength());
        Assert.Equal("Introduction and Greetings", lessons[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task CreateCourse_WhenStudent_ShouldReturnForbidden()
    {
        // Arrange
        var token = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, token);

        var courseRequest = new
        {
            Name = "Unauthorized Course",
            Description = "This course should not be created",
            Price = 99.90M,
            Content = new
            {
                Description = "Course content",
                Goals = "Course objectives",
                Requirements = "Course requirements"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/courses", courseRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCourses_ShouldReturnAllCourses()
    {
        // Arrange - Primeiro criamos alguns cursos com o admin
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // Cria um curso para garantir que temos dados
        var courseRequest = new
        {
            Name = "Test Course For Listing",
            Description = "Test course description",
            Price = 149.90M,
            Content = new
            {
                Description = "Test content",
                Goals = "Test objectives",
                Requirements = "Test requirements"
            }
        };

        await _client.PostAsJsonAsync("/api/courses", courseRequest);

        // Act - Agora tentamos listar os cursos sem autenticação (deve ser público)
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        
        Assert.True(root.GetArrayLength() > 0, "The course list should not be empty");
    }
} 