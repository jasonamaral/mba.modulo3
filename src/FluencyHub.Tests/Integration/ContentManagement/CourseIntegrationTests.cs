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
            Name = "Inglês para Iniciantes",
            Description = "Curso básico de inglês para iniciantes",
            Price = 199.90M,
            Content = new
            {
                Description = "Neste curso você aprenderá o básico do inglês",
                Goals = "Falar inglês básico em situações cotidianas",
                Requirements = "Nenhum conhecimento prévio necessário"
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
        
        Assert.Equal("Inglês para Iniciantes", courseRoot.GetProperty("name").GetString());
        Assert.Equal("Curso básico de inglês para iniciantes", courseRoot.GetProperty("description").GetString());

        // Agora tenta adicionar uma aula ao curso
        var lessonRequest = new
        {
            Title = "Introdução e Saudações",
            Content = "Nesta aula você aprenderá as saudações mais comuns em inglês como: Hello, Hi, Good morning, etc.",
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
        Assert.Equal("Introdução e Saudações", lessons[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task CreateCourse_WhenStudent_ShouldReturnForbidden()
    {
        // Arrange
        var token = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, token);

        var courseRequest = new
        {
            Name = "Curso Não Autorizado",
            Description = "Este curso não deve ser criado",
            Price = 99.90M,
            Content = new
            {
                Description = "Conteúdo do curso",
                Goals = "Objetivos do curso",
                Requirements = "Requisitos do curso"
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
            Name = "Curso de Teste Para Listagem",
            Description = "Descrição do curso de teste",
            Price = 149.90M,
            Content = new
            {
                Description = "Conteúdo de teste",
                Goals = "Objetivos de teste",
                Requirements = "Requisitos de teste"
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
        
        Assert.True(root.GetArrayLength() > 0, "A lista de cursos não deve estar vazia");
    }
} 