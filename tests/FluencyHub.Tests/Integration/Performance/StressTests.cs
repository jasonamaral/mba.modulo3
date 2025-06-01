using FluencyHub.Tests.Helpers;
using FluentAssertions;
using System.Diagnostics;
using System.Net;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.ContentManagement.Infrastructure.Persistence;

namespace FluencyHub.Tests.Integration.Performance;

public class StressTests : IntegrationTestBase
{

    [Fact]
    public async Task DatabaseConnections_ShouldHandleMaxConnections()
    {
        // Arrange
        var maxConnections = 100;
        var tasks = new List<Task>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < maxConnections; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var scope = Factory.Services.CreateScope();
                var studentContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
                var contentContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
                
                // Simular operações de banco de dados
                var students = studentContext.Students.Take(1).ToList();
                var courses = contentContext.Courses.Take(1).ToList();
                
                await Task.Delay(100); // Simular processamento
            }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // Menos de 30 segundos
    }

    [Fact]
    public async Task MemoryUsage_ShouldNotExceedLimits_WhenProcessingLargeDatasets()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var largeDatasetSize = 1000;

        // Act
        var students = new List<object>();
        for (int i = 0; i < largeDatasetSize; i++)
        {
            students.Add(new
            {
                Id = Guid.NewGuid(),
                FirstName = $"Student{i}",
                LastName = $"LastName{i}",
                Email = $"student{i}@example.com",
                DateOfBirth = DateTime.Now.AddYears(-20),
                CreatedAt = DateTime.UtcNow
            });
        }

        // Simular processamento dos dados
        var processedStudents = students
            .Where(s => s.GetType().GetProperty("Email")?.GetValue(s)?.ToString()?.Contains("@") == true)
            .ToList();

        var finalMemory = GC.GetTotalMemory(false);
        var memoryUsed = finalMemory - initialMemory;

        // Assert
        processedStudents.Should().HaveCount(largeDatasetSize);
        var memoryUsedMB = (int)(memoryUsed / 1024.0 / 1024.0);
        memoryUsedMB.Should().BeLessThan(50); // Menos de 50MB

        // Cleanup
        students.Clear();
        processedStudents.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    [Fact]
    public async Task HighVolumeRequests_ShouldMaintainPerformance()
    {
        // Arrange
        var requestCount = 500;
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(Client.GetAsync("/api/cursos"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().HaveCount(requestCount);
        responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().BeGreaterThan((int)(requestCount * 0.95)); // 95% de sucesso
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000); // Menos de 1 minuto
    }

    [Fact]
    public async Task ConcurrentDatabaseOperations_ShouldNotCauseDeadlocks()
    {
        // Arrange
        var operationCount = 50;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < operationCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                using var scope = Factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
                
                var student = TestDataBuilder.CreateValidStudent(email: $"stress{index}@example.com");
                TestDataBuilder.SetEntityId(student, Guid.NewGuid());
                
                context.Students.Add(student);
                await context.SaveChangesAsync();
                
                // Simular leitura após escrita
                var savedStudent = await context.Students.FindAsync(student.Id);
                savedStudent.Should().NotBeNull();
            }));
        }

        // Assert - Não deve lançar exceções de deadlock
        await Task.WhenAll(tasks);
    }
} 