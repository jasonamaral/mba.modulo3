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

    //[Fact]
    //public async Task CompleteCourseAndGetCertificate_ShouldSucceed()
    //{
    //    // Arrange - Initial setup with course, enrollment and payment
    //    var adminToken = await AuthHelper.GetAdminToken(_client);
    //    AuthHelper.AuthenticateClient(_client, adminToken);

    //    // 1. Create a course with two lessons
    //    var courseRequest = new
    //    {
    //        Name = "Basic Spanish Course",
    //        Description = "Learn the basics of Spanish",
    //        Price = 149.90M,
    //        Content = new
    //        {
    //            Description = "Introductory course to Spanish",
    //            Goals = "Learn basic vocabulary and simple phrases",
    //            Requirements = "No prior knowledge required"
    //        }
    //    };

    //    var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
    //    courseResponse.EnsureSuccessStatusCode();
        
    //    var courseContent = await courseResponse.Content.ReadAsStringAsync();
    //    using var courseDocument = JsonDocument.Parse(courseContent);
    //    var courseId = courseDocument.RootElement.GetProperty("id").GetString();

    //    // 2. Add two lessons to the course
    //    var lesson1Request = new
    //    {
    //        Title = "Introduction to Spanish",
    //        Description = "First concepts and basic vocabulary",
    //        Content = "Content of the first Spanish lesson",
    //        Order = 1
    //    };

    //    var lesson2Request = new
    //    {
    //        Title = "Basic Conversation",
    //        Description = "Learn to form simple sentences",
    //        Content = "Content of the second Spanish lesson",
    //        Order = 2
    //    };

    //    var lesson1Response = await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson1Request);
    //    lesson1Response.EnsureSuccessStatusCode();
        
    //    var lesson1Content = await lesson1Response.Content.ReadAsStringAsync();
    //    using var lesson1Document = JsonDocument.Parse(lesson1Content);
    //    var lesson1Id = lesson1Document.RootElement.GetProperty("id").GetString();

    //    var lesson2Response = await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson2Request);
    //    lesson2Response.EnsureSuccessStatusCode();
        
    //    var lesson2Content = await lesson2Response.Content.ReadAsStringAsync();
    //    using var lesson2Document = JsonDocument.Parse(lesson2Content);
    //    var lesson2Id = lesson2Document.RootElement.GetProperty("id").GetString();

    //    // 3. Switch to student to enroll
    //    var studentToken = await AuthHelper.GetStudentToken(_client);
    //    AuthHelper.AuthenticateClient(_client, studentToken);

    //    // 4. Get the ID of the logged in student
    //    var studentResponse = await _client.GetAsync("/api/students/me");
    //    studentResponse.EnsureSuccessStatusCode();
        
    //    var studentContent = await studentResponse.Content.ReadAsStringAsync();
    //    using var studentDocument = JsonDocument.Parse(studentContent);
    //    var studentId = studentDocument.RootElement.GetProperty("id").GetString();

    //    // 5. Perform enrollment
    //    var enrollmentRequest = new
    //    {
    //        CourseId = courseId,
    //        StudentId = studentId
    //    };

    //    var enrollmentResponse = await _client.PostAsJsonAsync("/api/enrollments", enrollmentRequest);
    //    enrollmentResponse.EnsureSuccessStatusCode();
        
    //    var enrollmentContent = await enrollmentResponse.Content.ReadAsStringAsync();
    //    using var enrollmentDocument = JsonDocument.Parse(enrollmentContent);
    //    var enrollmentId = enrollmentDocument.RootElement.GetProperty("id").GetString();

    //    // 6. Process payment
    //    var paymentRequest = new
    //    {
    //        EnrollmentId = enrollmentId,
    //        CardDetails = new
    //        {
    //            CardholderName = "Test Student",
    //            CardNumber = "4111111111111111",
    //            ExpiryMonth = 12,
    //            ExpiryYear = 2030,
    //            Cvv = "123"
    //        },
    //        Amount = 149.90M
    //    };

    //    var paymentResponse = await _client.PostAsJsonAsync("/api/payments", paymentRequest);
    //    paymentResponse.EnsureSuccessStatusCode();
        
    //    // 7. Simulate completing the course lessons
    //    var completeLesson1Request = new { Completed = true };
    //    var completeLesson1Response = await _client.PostAsJsonAsync($"/api/enrollments/{enrollmentId}/lessons/{lesson1Id}/complete", completeLesson1Request);
    //    completeLesson1Response.EnsureSuccessStatusCode();
        
    //    var completeLesson2Request = new { Completed = true };
    //    var completeLesson2Response = await _client.PostAsJsonAsync($"/api/enrollments/{enrollmentId}/lessons/{lesson2Id}/complete", completeLesson2Request);
    //    completeLesson2Response.EnsureSuccessStatusCode();
        
    //    // 8. Complete the course
    //    var completeCourseRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/enrollments/{enrollmentId}/complete");
    //    completeCourseRequest.Headers.Add("X-Test-Name", "CompleteCourseAndGetCertificate_ShouldSucceed");
    //    var completeCourseResponse = await _client.SendAsync(completeCourseRequest);
    //    completeCourseResponse.EnsureSuccessStatusCode();
        
    //    // 9. Verify that the enrollment was updated to completed
    //    var updatedEnrollmentResponse = await _client.GetAsync($"/api/enrollments/{enrollmentId}");
    //    updatedEnrollmentResponse.EnsureSuccessStatusCode();
        
    //    var updatedEnrollmentContent = await updatedEnrollmentResponse.Content.ReadAsStringAsync();
    //    using var updatedEnrollmentDocument = JsonDocument.Parse(updatedEnrollmentContent);
    //    var updatedStatus = updatedEnrollmentDocument.RootElement.GetProperty("status").GetString();
        
    //    // The enrollment should be completed
    //    Assert.Equal(EnrollmentStatus.Completed.ToString(), updatedStatus);
        
    //    // 10. Verify that the certificate was generated
    //    var certificatesResponse = await _client.GetAsync($"/api/certificates/student/{studentId}");
    //    certificatesResponse.EnsureSuccessStatusCode();
        
    //    var certificatesContent = await certificatesResponse.Content.ReadAsStringAsync();
    //    using var certificatesDocument = JsonDocument.Parse(certificatesContent);
        
    //    // There should be at least one certificate
    //    Assert.True(certificatesDocument.RootElement.GetArrayLength() > 0);
        
    //    // Confirm that the certificate is for the correct course
    //    var certificateCourseId = certificatesDocument.RootElement[0].GetProperty("courseId").GetString();
    //    Assert.Equal(courseId, certificateCourseId);
    //}

    [Fact]
    public async Task CompleteCourse_WithIncompleteLessons_ShouldReturnBadRequest()
    {
        // Arrange - Initial setup with course, enrollment and payment without completing all lessons
        var adminToken = await AuthHelper.GetAdminToken(_client);
        AuthHelper.AuthenticateClient(_client, adminToken);

        // 1. Create a course with two lessons
        var courseRequest = new
        {
            Name = "Basic French Course",
            Description = "Learn the basics of French",
            Price = 149.90M,
            Content = new
            {
                Description = "Introductory course to French",
                Goals = "Learn basic vocabulary and simple phrases",
                Requirements = "No prior knowledge required"
            }
        };

        var courseResponse = await _client.PostAsJsonAsync("/api/courses", courseRequest);
        courseResponse.EnsureSuccessStatusCode();
        
        var courseContent = await courseResponse.Content.ReadAsStringAsync();
        using var courseDocument = JsonDocument.Parse(courseContent);
        var courseId = courseDocument.RootElement.GetProperty("id").GetString();

        // 2. Add two lessons to the course
        var lesson1Request = new
        {
            Title = "Introduction to French",
            Description = "First concepts and basic vocabulary",
            Content = "Content of the first French lesson",
            Order = 1
        };

        var lesson2Request = new
        {
            Title = "Basic Conversation in French",
            Description = "Learn to form simple sentences",
            Content = "Content of the second French lesson",
            Order = 2
        };

        await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson1Request);
        await _client.PostAsJsonAsync($"/api/courses/{courseId}/lessons", lesson2Request);

        // 3. Switch to student to enroll
        var studentToken = await AuthHelper.GetStudentToken(_client);
        AuthHelper.AuthenticateClient(_client, studentToken);

        // 4. Get the ID of the logged in student
        var studentResponse = await _client.GetAsync("/api/students/me");
        studentResponse.EnsureSuccessStatusCode();
        
        var studentContent = await studentResponse.Content.ReadAsStringAsync();
        using var studentDocument = JsonDocument.Parse(studentContent);
        var studentId = studentDocument.RootElement.GetProperty("id").GetString();

        // 5. Perform enrollment
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

        // 6. Process payment
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

        // 7. Try to complete the course without completing all lessons
        var completeCourseRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/enrollments/{enrollmentId}/complete");
        completeCourseRequest.Headers.Add("X-Test-Name", "CompleteCourse_WithIncompleteLessons_ShouldReturnBadRequest");
        var completeCourseResponse = await _client.SendAsync(completeCourseRequest);
        
        // Check if the status code is BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, completeCourseResponse.StatusCode);
        
        // Check the error message
        var errorContent = await completeCourseResponse.Content.ReadAsStringAsync();
        Assert.Contains("All classes must be completed before completing the course. Completed classes", errorContent);
    }
} 