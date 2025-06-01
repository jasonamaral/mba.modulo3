using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FluencyHub.Tests.Helpers;

public class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    private readonly string _testDatabaseId;

    public IntegrationTestBase()
    {
        // Criar um ID único para cada instância de teste com timestamp mais preciso
        _testDatabaseId = $"{Guid.NewGuid():N}_{DateTime.UtcNow.Ticks}_{Environment.TickCount}";
        
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["JwtSettings:Secret"] = "SuperSecretKeyForJwtTokenThatIsLongEnoughForTesting123456789",
                        ["JwtSettings:Issuer"] = "FluencyHub",
                        ["JwtSettings:Audience"] = "FluencyHub",
                        ["JwtSettings:ExpiryInDays"] = "7",
                        ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                        ["PaymentGateway:ApiKey"] = "test-key",
                        ["PaymentGateway:BaseUrl"] = "https://test-gateway.com"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Remove os contextos reais e adiciona contextos em memória
                    RemoveService<DbContextOptions<StudentDbContext>>(services);
                    RemoveService<DbContextOptions<ContentDbContext>>(services);
                    RemoveService<DbContextOptions<PaymentDbContext>>(services);

                    // Adiciona contextos em memória com nome único por instância de teste
                    services.AddDbContext<StudentDbContext>(options =>
                        options.UseInMemoryDatabase($"StudentDb_{_testDatabaseId}")
                               .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
                    
                    services.AddDbContext<ContentDbContext>(options =>
                        options.UseInMemoryDatabase($"ContentDb_{_testDatabaseId}")
                               .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
                    
                    services.AddDbContext<PaymentDbContext>(options =>
                        options.UseInMemoryDatabase($"PaymentDb_{_testDatabaseId}")
                               .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                    // Adiciona Health Checks para testes
                    services.AddHealthChecks()
                        .AddCheck("test", () => HealthCheckResult.Healthy("Test health check"));

                    // Remove IIdentityService real e adiciona mock
                    RemoveService<FluencyHub.StudentManagement.Application.Common.Interfaces.IIdentityService>(services);
                    services.AddSingleton<FluencyHub.StudentManagement.Application.Common.Interfaces.IIdentityService, MockIdentityService>();

                    // Remove IPaymentGateway real e adiciona mock
                    RemoveService<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway>(services);
                    services.AddSingleton<FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway, MockPaymentGateway>();

                    // Remove autenticação JWT e adiciona autenticação de teste
                    var authDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthenticationSchemeProvider));
                    if (authDescriptor != null)
                    {
                        services.Remove(authDescriptor);
                    }

                    // Configura autenticação de teste como padrão
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                        options.DefaultScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                    
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
                        options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
                    });
                });
            });

        Client = Factory.CreateClient();
        
        // Inicializar os bancos de dados em memória
        InitializeDatabases();
    }

    private void InitializeDatabases()
    {
        using var scope = Factory.Services.CreateScope();
        
        var studentContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        var contentContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        var paymentContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        // Garantir que os bancos estão criados
        studentContext.Database.EnsureCreated();
        contentContext.Database.EnsureCreated();
        paymentContext.Database.EnsureCreated();
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    protected HttpClient CreateAuthenticatedClient(string email = "test@example.com", string role = "Student")
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", $"{email}|{role}");
        return client;
    }

    protected HttpClient CreateAdminClient(string email = "admin@example.com")
    {
        return CreateAuthenticatedClient(email, "Administrator");
    }

    protected HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    protected async Task SeedTestDataAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var studentContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        var contentContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        var paymentContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        // Seed students com IDs únicos e emails únicos
        var uniqueId1 = Guid.NewGuid();
        var student = TestDataBuilder.CreateValidStudent("João", "Silva", $"joao.silva.{uniqueId1}@email.com");
        TestDataBuilder.SetEntityId(student, uniqueId1);
        await studentContext.Students.AddAsync(student);

        var uniqueId2 = Guid.NewGuid();
        var student2 = TestDataBuilder.CreateValidStudent("Maria", "Santos", $"maria.santos.{uniqueId2}@email.com");
        TestDataBuilder.SetEntityId(student2, uniqueId2);
        await studentContext.Students.AddAsync(student2);

        await studentContext.SaveChangesAsync();

        // Seed courses com IDs únicos
        var courseId1 = Guid.NewGuid();
        var course = TestDataBuilder.CreateValidCourse("Inglês Básico", "Curso de inglês para iniciantes", 299.99m);
        TestDataBuilder.SetEntityId(course, courseId1);
        await contentContext.Courses.AddAsync(course);

        var courseId2 = Guid.NewGuid();
        var course2 = TestDataBuilder.CreateValidCourse("Inglês Avançado", "Curso de inglês avançado", 499.99m);
        TestDataBuilder.SetEntityId(course2, courseId2);
        await contentContext.Courses.AddAsync(course2);

        await contentContext.SaveChangesAsync();
    }

    protected async Task<Guid> CreateTestStudentAsync(string email = "test.student@email.com")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        
        var uniqueId = Guid.NewGuid();
        var student = TestDataBuilder.CreateValidStudent("Test", "Student", $"{uniqueId}_{email}");
        TestDataBuilder.SetEntityId(student, uniqueId);
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();
        
        return student.Id;
    }

    protected async Task<Guid> CreateTestCourseAsync(string name = "Test Course")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        
        var uniqueId = Guid.NewGuid();
        var course = TestDataBuilder.CreateValidCourse($"{name}_{uniqueId}", "Test course description", 199.99m);
        TestDataBuilder.SetEntityId(course, uniqueId);
        await context.Courses.AddAsync(course);
        await context.SaveChangesAsync();
        
        return course.Id;
    }

    protected async Task ClearDatabasesAsync()
    {
        using var scope = Factory.Services.CreateScope();
        
        var studentContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        var contentContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        var paymentContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        // Limpar todas as entidades
        studentContext.Students.RemoveRange(studentContext.Students);
        studentContext.Enrollments.RemoveRange(studentContext.Enrollments);
        studentContext.Certificates.RemoveRange(studentContext.Certificates);
        
        contentContext.Courses.RemoveRange(contentContext.Courses);
        contentContext.Lessons.RemoveRange(contentContext.Lessons);
        
        paymentContext.Payments.RemoveRange(paymentContext.Payments);

        await studentContext.SaveChangesAsync();
        await contentContext.SaveChangesAsync();
        await paymentContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        try
        {
            // Limpar os bancos de dados antes de fazer dispose
            ClearDatabasesAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // Ignorar erros durante limpeza
        }
        
        Client?.Dispose();
        Factory?.Dispose();
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Test "))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var authData = authHeader.Substring("Test ".Length);
        var parts = authData.Split('|');
        var email = parts[0];
        var role = parts.Length > 1 ? parts[1] : "Student";

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("email", email),
            new Claim("role", role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class MockIdentityService : FluencyHub.StudentManagement.Application.Common.Interfaces.IIdentityService
{
    public Task<FluencyHub.StudentManagement.Application.Common.Models.AuthResult> AuthenticateAsync(string email, string password)
    {
        // Simula falha para credenciais inválidas
        if (email == "invalid@example.com" || password == "wrongpassword")
        {
            var failResult = FluencyHub.StudentManagement.Application.Common.Models.AuthResult.Failure("Invalid credentials");
            return Task.FromResult(failResult);
        }
        
        var result = FluencyHub.StudentManagement.Application.Common.Models.AuthResult.Success("mock-jwt-token");
        return Task.FromResult(result);
    }

    public Task<FluencyHub.StudentManagement.Application.Common.Models.AuthResult> RegisterUserAsync(string email, string password, string firstName, string lastName)
    {
        // Simula falha para email já existente
        if (email == "existing@example.com")
        {
            var failResult = FluencyHub.StudentManagement.Application.Common.Models.AuthResult.Failure("Email already exists");
            return Task.FromResult(failResult);
        }
        
        var result = FluencyHub.StudentManagement.Application.Common.Models.AuthResult.Success("mock-jwt-token");
        return Task.FromResult(result);
    }

    public Task<bool> UpdateUserStudentIdAsync(string email, Guid studentId)
    {
        return Task.FromResult(true);
    }

    public Task<bool> DeleteUserAsync(string email)
    {
        return Task.FromResult(true);
    }

    public Task<bool> AddToRoleAsync(string email, string roleName)
    {
        return Task.FromResult(true);
    }

    public Task<bool> EnsureRoleExistsAsync(string roleName)
    {
        return Task.FromResult(true);
    }
}

public class MockPaymentGateway : FluencyHub.PaymentProcessing.Application.Common.Interfaces.IPaymentGateway
{
    public Task<FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult> ProcessPaymentAsync(string orderId, decimal amount, FluencyHub.PaymentProcessing.Application.Common.Models.CardDetails cardDetails)
    {
        // Simula falha para amounts zero ou negativos
        if (amount <= 0)
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Valor inválido para pagamento");
            return Task.FromResult(failResult);
        }

        // Simula falha para cartões específicos
        if (cardDetails.CardNumber == "4000000000000002") // Cartão expirado
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Cartão expirado ou recusado");
            return Task.FromResult(failResult);
        }
        
        if (cardDetails.CardNumber == "4000000000000069") // Cartão inválido
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Cartão inválido ou recusado");
            return Task.FromResult(failResult);
        }
        
        if (cardDetails.CardNumber == "1234567890123456") // Cartão inválido usado no teste
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Cartão inválido ou recusado");
            return Task.FromResult(failResult);
        }
        
        // Simula falha para cartões que terminam com 0000 (exceto teste válido)
        if (cardDetails.CardNumber.EndsWith("0000") && cardDetails.CardNumber != "4532015112830366")
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Cartão inválido ou recusado");
            return Task.FromResult(failResult);
        }
        
        // Simula falha para cartões que terminam com 0001 (cartão de teste inválido)
        if (cardDetails.CardNumber.EndsWith("0001"))
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Cartão inválido ou recusado");
            return Task.FromResult(failResult);
        }
        
        if (cardDetails.ExpiryYear == "2020") // Ano expirado
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Cartão expirado ou recusado");
            return Task.FromResult(failResult);
        }

        // Simula timeout para CVV específico
        if (cardDetails.Cvv == "999")
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Timeout ou gateway indisponível");
            return Task.FromResult(failResult);
        }

        // Simula falha para números de cartão inválidos (não numericos ou muito curtos)
        if (string.IsNullOrEmpty(cardDetails.CardNumber) || cardDetails.CardNumber.Length < 13 || !cardDetails.CardNumber.All(char.IsDigit))
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Dados do cartão inválidos");
            return Task.FromResult(failResult);
        }

        // Simula falha para orderId inválido ou timeout simulado
        if (string.IsNullOrEmpty(orderId) || orderId.Contains("timeout") || orderId.Contains("unavailable"))
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Gateway indisponível ou timeout");
            return Task.FromResult(failResult);
        }
        
        // Simula falha para nome de portador específico que deve falhar nos testes
        if (cardDetails.CardHolderName == "INVALID TEST" || cardDetails.CardHolderName == "TIMEOUT TEST")
        {
            var failResult = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Failure("Dados do cartão inválidos");
            return Task.FromResult(failResult);
        }
        
        var result = FluencyHub.PaymentProcessing.Application.Common.Models.PaymentResult.Success("mock-transaction-id");
        return Task.FromResult(result);
    }

    public Task<FluencyHub.PaymentProcessing.Application.Common.Models.RefundResult> ProcessRefundAsync(string transactionId, decimal amount, string reason)
    {
        var result = FluencyHub.PaymentProcessing.Application.Common.Models.RefundResult.Success(transactionId, "mock-refund-id", amount);
        return Task.FromResult(result);
    }
} 