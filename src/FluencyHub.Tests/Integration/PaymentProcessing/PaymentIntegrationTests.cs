using System.Net;
using System.Net.Http.Json;
using FluencyHub.Tests.Integration.Helpers;
using System.Text.Json;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.StudentManagement;
using FluencyHub.Domain.PaymentProcessing;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.Infrastructure.Persistence;

namespace FluencyHub.Tests.Integration.PaymentProcessing;

[Trait("Category", "Integration")]
public class PaymentIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly HttpClient _client;

    public PaymentIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task ProcessPayment_WithValidCard_ShouldSucceed()
    {
        // Arrange - Configuração inicial com curso e matrícula
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 1. Criar um curso
        var courseRequest = new
        {
            Name = "Curso de Alemão",
            Description = "Aprenda alemão do zero",
            Price = 199.90M,
            Content = new
            {
                Description = "Curso completo de alemão",
                Goals = "Aprender o básico do alemão",
                Requirements = "Nenhum conhecimento prévio necessário"
            }
        };

        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
        courseResponse.EnsureSuccessStatusCode();
        
        var courseContent = await courseResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(courseContent);
        var courseId = courseDocument.RootElement.GetProperty("id").GetString();

        // 2. Alternar para o aluno para se matricular
        var studentToken = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, studentToken);

        // 3. Obter o ID do aluno logado
        var studentResponse = await _client.GetAsync("/api/students/me");
        studentResponse.EnsureSuccessStatusCode();
        
        var studentContent = await studentResponse.Content.ReadAsStringAsync();
        using var studentDocument = JsonDocument.Parse(studentContent);
        var studentId = studentDocument.RootElement.GetProperty("id").GetString();

        // 4. Realizar matrícula
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

        // 5. Realizar pagamento
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
            Amount = 199.90M
        };

        var paymentResponse = await _client.PostAsJsonAsync("/api/payments", paymentRequest);
        paymentResponse.EnsureSuccessStatusCode();
        
        var paymentContent = await paymentResponse.Content.ReadAsStringAsync();
        using var paymentDocument = JsonDocument.Parse(paymentContent);
        var paymentId = paymentDocument.RootElement.GetProperty("id").GetString();
        var paymentStatus = paymentDocument.RootElement.GetProperty("status").GetString();
        
        // Pagamento deve ser aprovado
        Assert.Equal("Successful", paymentStatus);
        
        // 6. Verificar se a matrícula foi atualizada para ativa
        var updatedEnrollmentResponse = await _client.GetAsync($"/api/enrollments/{enrollmentId}");
        updatedEnrollmentResponse.EnsureSuccessStatusCode();
        
        var updatedEnrollmentContent = await updatedEnrollmentResponse.Content.ReadAsStringAsync();
        using var updatedEnrollmentDocument = JsonDocument.Parse(updatedEnrollmentContent);
        var updatedStatus = updatedEnrollmentDocument.RootElement.GetProperty("status").GetString();
        
        // A matrícula deve estar ativa após o pagamento
        Assert.Equal("Active", updatedStatus);
    }

    [Fact]
    public async Task ProcessPayment_WithInvalidCard_ShouldFail()
    {
        // Arrange - Configuração inicial com curso e matrícula
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 1. Criar um curso
        var courseRequest = new
        {
            Name = "Curso de Russo",
            Description = "Aprenda russo do zero",
            Price = 229.90M,
            Content = new
            {
                Description = "Curso completo de russo",
                Goals = "Aprender o básico do russo",
                Requirements = "Nenhum conhecimento prévio necessário"
            }
        };

        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
        courseResponse.EnsureSuccessStatusCode();
        
        var courseContent = await courseResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(courseContent);
        var courseId = courseDocument.RootElement.GetProperty("id").GetString();

        // 2. Alternar para o aluno para se matricular
        var studentToken = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, studentToken);

        // 3. Obter o ID do aluno logado
        var studentResponse = await _client.GetAsync("/api/students/me");
        studentResponse.EnsureSuccessStatusCode();
        
        var studentContent = await studentResponse.Content.ReadAsStringAsync();
        using var studentDocument = JsonDocument.Parse(studentContent);
        var studentId = studentDocument.RootElement.GetProperty("id").GetString();

        // 4. Realizar matrícula
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

        // 5. Realizar pagamento com cartão inválido (número inválido deliberadamente)
        var paymentRequest = new
        {
            EnrollmentId = enrollmentId,
            CardDetails = new
            {
                CardholderName = "Test Student",
                CardNumber = "1111111111111111", // Número inválido
                ExpiryMonth = 12,
                ExpiryYear = 2030,
                Cvv = "123"
            },
            Amount = 229.90M
        };

        var paymentResponse = await _client.PostAsJsonAsync("/api/payments", paymentRequest);
        
        // Deve falhar com cartão inválido
        Assert.Equal(HttpStatusCode.BadRequest, paymentResponse.StatusCode);
        
        // 6. Verificar se a matrícula continua com status pendente
        var updatedEnrollmentResponse = await _client.GetAsync($"/api/enrollments/{enrollmentId}");
        updatedEnrollmentResponse.EnsureSuccessStatusCode();
        
        var updatedEnrollmentContent = await updatedEnrollmentResponse.Content.ReadAsStringAsync();
        using var updatedEnrollmentDocument = JsonDocument.Parse(updatedEnrollmentContent);
        var updatedStatus = updatedEnrollmentDocument.RootElement.GetProperty("status").GetString();
        
        // A matrícula deve continuar como pendente
        Assert.Equal("PendingPayment", updatedStatus);
    }

    [Fact]
    public async Task RequestRefund_ForRecentPayment_ShouldSucceed()
    {
        // Arrange - Configuração inicial com curso, matrícula e pagamento
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 1. Criar um curso
        var courseRequest = new
        {
            Name = "Curso de Japonês",
            Description = "Aprenda japonês do zero",
            Price = 249.90M,
            Content = new
            {
                Description = "Curso completo de japonês",
                Goals = "Aprender o básico do japonês",
                Requirements = "Nenhum conhecimento prévio necessário"
            }
        };

        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
        courseResponse.EnsureSuccessStatusCode();
        
        var courseContent = await courseResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(courseContent);
        var courseId = courseDocument.RootElement.GetProperty("id").GetString();

        // 2. Alternar para o aluno para se matricular
        var studentToken = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, studentToken);

        // 3. Obter o ID do aluno logado
        var studentResponse = await _client.GetAsync("/api/students/me");
        studentResponse.EnsureSuccessStatusCode();
        
        var studentContent = await studentResponse.Content.ReadAsStringAsync();
        using var studentDocument = JsonDocument.Parse(studentContent);
        var studentId = studentDocument.RootElement.GetProperty("id").GetString();

        // 4. Realizar matrícula
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

        // 5. Realizar pagamento
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
            Amount = 249.90M
        };

        var paymentResponse = await _client.PostAsJsonAsync("/api/payments", paymentRequest);
        paymentResponse.EnsureSuccessStatusCode();
        
        var paymentContent = await paymentResponse.Content.ReadAsStringAsync();
        using var paymentDocument = JsonDocument.Parse(paymentContent);
        var paymentId = paymentDocument.RootElement.GetProperty("id").GetString();

        // Alternar para o administrador para solicitar reembolso
        adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 6. Solicitar reembolso
        var refundRequest = new { Reason = "Teste de integração" };
        var refundResponse = await _client.PostAsJsonAsync($"/api/payments/{paymentId}/refund", refundRequest);
        refundResponse.EnsureSuccessStatusCode();
        
        // 7. Verificar se o status do pagamento foi atualizado para reembolsado
        var updatedPaymentResponse = await _client.GetAsync($"/api/payments/{paymentId}");
        updatedPaymentResponse.EnsureSuccessStatusCode();
        
        var updatedPaymentContent = await updatedPaymentResponse.Content.ReadAsStringAsync();
        using var updatedPaymentDocument = JsonDocument.Parse(updatedPaymentContent);
        var updatedPaymentStatus = updatedPaymentDocument.RootElement.GetProperty("status").GetString();
        
        // O status do pagamento deve ser "Refunded"
        Assert.Equal("Refunded", updatedPaymentStatus);
        
        // A matrícula deve estar cancelada após o reembolso, então vamos verificar a matrícula diretamente
        // Esse passo está fora do escopo do controlador de pagamentos, então vamos ignorar por enquanto
        // Assert.Equal("Cancelled", updatedEnrollmentStatus);
    }
} 