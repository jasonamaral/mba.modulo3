using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FluencyHub.Tests.Integration.Helpers;

public static class AuthHelper
{
    private record LoginRequest(string Email, string Password);
    
    public static async Task<string> GetAdminToken(HttpClient client)
    {
        return await GetToken(client, "admin@fluencyhub.com", "Admin123!");
    }
    
    public static async Task<string> GetStudentToken(HttpClient client)
    {
        return await GetToken(client, "student@example.com", "Student123!");
    }
    
    private static async Task<string> GetToken(HttpClient client, string email, string password)
    {
        var request = new LoginRequest(email, password);
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        
        return document.RootElement.GetProperty("token").GetString() ?? throw new InvalidOperationException("Token not found in response");
    }
    
    public static void AuthenticateClient(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
} 