using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FluencyHub.Infrastructure.Persistence;
using FluencyHub.Infrastructure.Identity;
using System.Transactions;
using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace FluencyHub.Tests.Integration.Config;

public abstract class IntegrationTestsBase<TProgram> : IDisposable where TProgram : class
{
    public readonly WebApplicationFactory<TProgram> Factory;
    public HttpClient Client;
    protected readonly SqliteConnection Connection;
    protected readonly TransactionScope TransactionScope;
    private readonly ConcurrentDictionary<string, string> _tokens = new();
    public string? CurrentToken { get; set; }

    protected IntegrationTestsBase(WebApplicationFactory<TProgram> factory)
    {
        Factory = factory;
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();
        
        // Configurar o banco de dados para cada teste
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FluencyHubDbContext>();
        dbContext.Database.EnsureCreated();
        
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
        var dbContext = scope.ServiceProvider.GetRequiredService<FluencyHubDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        
        // Seed dados iniciais
        await SeedTestDataAsync(dbContext);
    }

    protected virtual async Task SeedTestDataAsync(FluencyHubDbContext dbContext)
    {
        // Implementação padrão vazia - pode ser sobrescrita por classes derivadas
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        TransactionScope.Dispose();
        Connection?.Dispose();
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
} 