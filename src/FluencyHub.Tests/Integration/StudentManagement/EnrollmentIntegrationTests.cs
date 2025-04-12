using System.Net;
using System.Net.Http.Json;
using FluencyHub.Tests.Integration.Helpers;
using System.Text.Json;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.StudentManagement;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.Infrastructure.Persistence;

namespace FluencyHub.Tests.Integration.StudentManagement;

[Trait("Category", "Integration")]
public class EnrollmentIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public EnrollmentIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task CompleteEnrollmentFlow_FromCourseToPayment_ShouldSucceed()
    {
        // Arrange - Primeiro criamos um curso com o admin
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 1. Criar um curso
        var courseRequest = new
        {
            Name = "Inglês Intermediário",
            Description = "Curso intermediário de inglês",
            Price = 299.90M,
            Content = new
            {
                Description = "Aprofunde seus conhecimentos em inglês",
                Goals = "Comunicar-se em situações intermediárias",
                Requirements = "Conhecimento básico de inglês"
            }
        };

        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
        courseResponse.EnsureSuccessStatusCode();
        
        var courseContent = await courseResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(courseContent);
        var courseId = courseDocument.RootElement.GetProperty("id").GetString();

        // 2. Adicionar uma aula ao curso
        var lessonRequest = new
        {
            Title = "Tempos Verbais Avançados",
            Description = "Aprenda os tempos verbais mais complexos do inglês",
            Content = "Nesta aula você aprenderá os tempos verbais mais avançados do inglês.",
            Order = 1
        };

        await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lessonRequest);

        // 3. Alternar para o aluno para se matricular
        var studentToken = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, studentToken);

        // 4. Primeiro, obter o ID do aluno logado
        var studentResponse = await _client.GetAsync("/api/students/me");
        studentResponse.EnsureSuccessStatusCode();
        
        var studentContent = await studentResponse.Content.ReadAsStringAsync();
        using var studentDocument = JsonDocument.Parse(studentContent);
        var studentId = studentDocument.RootElement.GetProperty("id").GetString();

        // 5. Realizar matrícula
        var enrollmentRequest = new
        {
            CourseId = courseId,
            StudentId = studentId
        };

        var enrollmentResponse = await _client.PostAsJsonAsync("/api/enrollments", enrollmentRequest);
        enrollmentResponse.EnsureSuccessStatusCode();
        
        var enrollmentContent = await enrollmentResponse.Content.ReadAsStringAsync();
        using var enrollmentDocument = JsonDocument.Parse(enrollmentContent);
        var enrollmentId = enrollmentDocument.RootElement.GetProperty("id").GetString();
        var enrollmentStatus = enrollmentDocument.RootElement.GetProperty("status").GetString();
        
        // A matrícula deve estar com status pendente antes do pagamento
        Assert.Equal("PendingPayment", enrollmentStatus);

        // 6. Realizar pagamento
        var paymentRequest = new
        {
            EnrollmentId = enrollmentId,
            CardDetails = new
            {
                CardholderName = "Test Student",
                CardNumber = "4111111111111111",
                ExpiryMonth = 12,
                ExpiryYear = 2030,
                Cvv = "123"
            },
            Amount = 299.90M
        };

        var paymentResponse = await _client.PostAsJsonAsync("/api/payments", paymentRequest);
        paymentResponse.EnsureSuccessStatusCode();
        
        // 7. Verificar se a matrícula foi atualizada para ativa após o pagamento
        var updatedEnrollmentResponse = await _client.GetAsync($"/api/enrollments/{enrollmentId}");
        updatedEnrollmentResponse.EnsureSuccessStatusCode();
        
        var updatedEnrollmentContent = await updatedEnrollmentResponse.Content.ReadAsStringAsync();
        using var updatedEnrollmentDocument = JsonDocument.Parse(updatedEnrollmentContent);
        var updatedStatus = updatedEnrollmentDocument.RootElement.GetProperty("status").GetString();
        
        // A matrícula deve estar ativa após o pagamento
        Assert.Equal("Active", updatedStatus);
        
        // 8. Verificar se o aluno tem acesso às aulas do curso
        var lessonsResponse = await _client.GetAsync($"/api/courses/{courseId}/lessons");
        lessonsResponse.EnsureSuccessStatusCode();
        
        var lessonsContent = await lessonsResponse.Content.ReadAsStringAsync();
        using var lessonsDocument = JsonDocument.Parse(lessonsContent);
        
        // Deve haver pelo menos uma aula disponível
        Assert.True(lessonsDocument.RootElement.GetArrayLength() > 0);
    }

    [Fact]
    public async Task EnrollInCourse_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange - Tentar matricular-se sem autenticação
        _client.DefaultRequestHeaders.Authorization = null;

        var enrollmentRequest = new
        {
            CourseId = Guid.NewGuid().ToString(),
            StudentId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/enrollments", enrollmentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
} 