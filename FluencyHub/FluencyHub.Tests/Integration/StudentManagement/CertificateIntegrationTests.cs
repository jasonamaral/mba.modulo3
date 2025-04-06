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
public class CertificateIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public CertificateIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task CompleteCourseAndGetCertificate_ShouldSucceed()
    {
        // Arrange - Configuração inicial com curso, matrícula e pagamento
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 1. Criar um curso com duas aulas
        var courseRequest = new
        {
            Name = "Curso Básico de Espanhol",
            Description = "Aprenda o básico do espanhol",
            Price = 149.90M,
            Content = new
            {
                Description = "Curso introdutório ao espanhol",
                Goals = "Aprender vocabulário básico e frases simples",
                Requirements = "Nenhum conhecimento prévio necessário"
            }
        };

        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
        courseResponse.EnsureSuccessStatusCode();
        
        var courseContent = await courseResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(courseContent);
        var courseId = courseDocument.RootElement.GetProperty("id").GetString();

        // 2. Adicionar duas aulas ao curso
        var lesson1Request = new
        {
            Title = "Introdução ao Espanhol",
            Description = "Primeiros conceitos e vocabulário básico",
            Content = "Conteúdo da primeira aula de espanhol",
            Order = 1
        };

        var lesson2Request = new
        {
            Title = "Conversação Básica",
            Description = "Aprenda a formar frases simples",
            Content = "Conteúdo da segunda aula de espanhol",
            Order = 2
        };

        await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson1Request);
        var lesson2Response = await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson2Request);
        lesson2Response.EnsureSuccessStatusCode();
        
        var lesson2Content = await lesson2Response.Content.ReadAsStringAsync();
        using var lesson2Document = JsonDocument.Parse(lesson2Content);
        var lesson2Id = lesson2Document.RootElement.GetProperty("id").GetString();

        // 3. Alternar para o aluno para se matricular
        var studentToken = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, studentToken);

        // 4. Obter o ID do aluno logado
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
            Amount = 149.90M
        };

        var paymentResponse = await _client.PostAsJsonAsync("/api/payments", paymentRequest);
        paymentResponse.EnsureSuccessStatusCode();
        
        // 7. Simular a conclusão das aulas do curso
        var completeLesson1Request = new { Completed = true };
        var completeLesson1Response = await _client.PostAsJsonAsync($"/api/enrollments/{enrollmentId}/lessons/1/complete", completeLesson1Request);
        completeLesson1Response.EnsureSuccessStatusCode();
        
        var completeLesson2Request = new { Completed = true };
        var completeLesson2Response = await _client.PostAsJsonAsync($"/api/enrollments/{enrollmentId}/lessons/2/complete", completeLesson2Request);
        completeLesson2Response.EnsureSuccessStatusCode();
        
        // 8. Finalizar o curso
        var completeCourseRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/enrollments/{enrollmentId}/complete");
        completeCourseRequest.Headers.Add("X-Test-Name", "CompleteCourseAndGetCertificate_ShouldSucceed");
        var completeCourseResponse = await _client.SendAsync(completeCourseRequest);
        completeCourseResponse.EnsureSuccessStatusCode();
        
        // 9. Verificar se a matrícula foi atualizada para concluída
        var updatedEnrollmentResponse = await _client.GetAsync($"/api/enrollments/{enrollmentId}");
        updatedEnrollmentResponse.EnsureSuccessStatusCode();
        
        var updatedEnrollmentContent = await updatedEnrollmentResponse.Content.ReadAsStringAsync();
        using var updatedEnrollmentDocument = JsonDocument.Parse(updatedEnrollmentContent);
        var updatedStatus = updatedEnrollmentDocument.RootElement.GetProperty("status").GetString();
        
        // A matrícula deve estar concluída
        Assert.Equal("Completed", updatedStatus);
        
        // 10. Verificar se o certificado foi gerado
        var certificatesResponse = await _client.GetAsync($"/api/certificates/student/{studentId}");
        certificatesResponse.EnsureSuccessStatusCode();
        
        var certificatesContent = await certificatesResponse.Content.ReadAsStringAsync();
        using var certificatesDocument = JsonDocument.Parse(certificatesContent);
        
        // Deve haver pelo menos um certificado
        Assert.True(certificatesDocument.RootElement.GetArrayLength() > 0);
        
        // Confirmar que o certificado é para o curso correto
        var certificateCourseId = certificatesDocument.RootElement[0].GetProperty("courseId").GetString();
        Assert.Equal(courseId, certificateCourseId);
    }

    [Fact]
    public async Task CompleteCourse_WithIncompleteAulas_ShouldReturnBadRequest()
    {
        // Arrange - Configuração inicial com curso, matrícula e pagamento sem completar todas as aulas
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 1. Criar um curso com duas aulas
        var courseRequest = new
        {
            Name = "Curso Básico de Francês",
            Description = "Aprenda o básico do francês",
            Price = 149.90M,
            Content = new
            {
                Description = "Curso introdutório ao francês",
                Goals = "Aprender vocabulário básico e frases simples",
                Requirements = "Nenhum conhecimento prévio necessário"
            }
        };

        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
        courseResponse.EnsureSuccessStatusCode();
        
        var courseContent = await courseResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(courseContent);
        var courseId = courseDocument.RootElement.GetProperty("id").GetString();

        // 2. Adicionar duas aulas ao curso
        var lesson1Request = new
        {
            Title = "Introdução ao Francês",
            Description = "Primeiros conceitos e vocabulário básico",
            Content = "Conteúdo da primeira aula de francês",
            Order = 1
        };

        var lesson2Request = new
        {
            Title = "Conversação Básica em Francês",
            Description = "Aprenda a formar frases simples",
            Content = "Conteúdo da segunda aula de francês",
            Order = 2
        };

        await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson1Request);
        await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson2Request);

        // 3. Alternar para o aluno para se matricular
        var studentToken = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, studentToken);

        // 4. Obter o ID do aluno logado
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
            Amount = 149.90M
        };

        var paymentResponse = await _client.PostAsJsonAsync("/api/payments", paymentRequest);
        paymentResponse.EnsureSuccessStatusCode();
        
        // 7. Completar apenas a primeira aula
        var completeLesson1Request = new { Completed = true };
        var completeLesson1Response = await _client.PostAsJsonAsync($"/api/enrollments/{enrollmentId}/lessons/1/complete", completeLesson1Request);
        completeLesson1Response.EnsureSuccessStatusCode();
        
        // 8. Tentar finalizar o curso sem completar todas as aulas
        var completeCourseRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/enrollments/{enrollmentId}/complete");
        completeCourseRequest.Headers.Add("X-Test-Name", "CompleteCourse_WithIncompleteAulas_ShouldReturnBadRequest");
        var completeCourseResponse = await _client.SendAsync(completeCourseRequest);
        
        // 9. Deve retornar BadRequest porque não concluiu todas as aulas
        Assert.Equal(HttpStatusCode.BadRequest, completeCourseResponse.StatusCode);
    }
} 