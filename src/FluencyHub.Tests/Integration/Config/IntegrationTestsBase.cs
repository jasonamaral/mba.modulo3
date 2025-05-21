using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.ContentManagement.Infrastructure.Persistence;
using FluencyHub.StudentManagement.Infrastructure.Persistence;
using FluencyHub.PaymentProcessing.Infrastructure.Persistence;
using FluencyHub.Infrastructure.Identity;
using System.Transactions;
using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace FluencyHub.Tests.Integration.Config;

public abstract class IntegrationTestsBase<TProgram> : IDisposable where TProgram : class
{
    public readonly WebApplicationFactory<TProgram> Factory;
    public HttpClient Client;
    protected readonly TransactionScope TransactionScope;
    private readonly ConcurrentDictionary<string, string> _tokens = new();
    public string? CurrentToken { get; set; }

    protected IntegrationTestsBase(WebApplicationFactory<TProgram> factory)
    {
        Factory = factory;
        
        // Iniciar uma transação para cada teste
        TransactionScope = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.Serializable },
            TransactionScopeAsyncFlowOption.Enabled);
        
        Client = Factory.CreateClient();
    }

    protected async Task<string> GetTokenAsync(string email, string password)
    {
        var key = $"{email}:{password}";
        if (_tokens.TryGetValue(key, out var token))
        {
            CurrentToken = token;
            return token;
        }

        var response = await Client.PostAsJsonAsync("api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = System.Text.Json.JsonDocument.Parse(content).RootElement;
        
        token = tokenResponse.GetProperty("token").GetString() 
            ?? throw new InvalidOperationException("Token not received");
            
        _tokens[key] = token;
        CurrentToken = token;
        return token;
    }

    protected void SetBearerToken()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", 
            CurrentToken ?? throw new InvalidOperationException("No token available. Call GetTokenAsync first."));
    }

    protected void ClearToken()
    {
        CurrentToken = null;
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        
        // Obter todos os DbContexts específicos de cada BC
        var contentDbContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        var studentDbContext = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
        var paymentDbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Redefinir cada contexto
        await contentDbContext.Database.EnsureDeletedAsync();
        await contentDbContext.Database.EnsureCreatedAsync();
        
        await studentDbContext.Database.EnsureDeletedAsync();
        await studentDbContext.Database.EnsureCreatedAsync();
        
        await paymentDbContext.Database.EnsureDeletedAsync();
        await paymentDbContext.Database.EnsureCreatedAsync();
        
        await identityDbContext.Database.EnsureDeletedAsync();
        await identityDbContext.Database.EnsureCreatedAsync();
        
        // Seed dados iniciais
        await SeedTestDataAsync(contentDbContext, studentDbContext, paymentDbContext);
    }

    protected virtual async Task SeedTestDataAsync(
        ContentDbContext contentDbContext, 
        StudentDbContext studentDbContext, 
        PaymentDbContext paymentDbContext)
    {
        // Implementação padrão vazia - pode ser sobrescrita por classes derivadas
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        TransactionScope.Dispose();
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
} 