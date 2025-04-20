using Bogus;
using FluencyHub.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluencyHub.Tests.Integration.Config;
[CollectionDefinition(nameof(IntegrationWebTestsFixtureCollection))]
public class IntegrationWebTestsFixtureCollection : ICollectionFixture<IntegrationTestsFixture<Program>> { }

[CollectionDefinition(nameof(IntegrationApiTestsFixtureCollection))]
public class IntegrationApiTestsFixtureCollection : ICollectionFixture<IntegrationTestsFixture<Program>> { }

public class IntegrationTestsFixture<TProgram> : IDisposable where TProgram : class
{
    public string AntiForgeryFieldName = "__RequestVerificationToken";

    public string UsuarioEmail;
    public string UsuarioSenha;

    public string UsuarioToken;

    public readonly FluencyHubAppFactory<TProgram> Factory;
    public HttpClient Client;

    public IntegrationTestsFixture()
    {
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            BaseAddress = new Uri("https://localhost:7152"),
            HandleCookies = true,
            MaxAutomaticRedirections = 7
        };

        Factory = new FluencyHubAppFactory<TProgram>();
        Client = Factory.CreateClient(clientOptions);
    }

    public void PasswordGenerate()
    {
        var faker = new Faker("pt_BR");
        UsuarioEmail = faker.Internet.Email().ToLower();
        UsuarioSenha = faker.Internet.Password(8, false, "", "@1Ab_");
    }

    public async Task DoLoginAsStudent()
    {
        var userData = new LoginRequest
        {
            Email = "jack.student1@fluencyhub.com",
            Password = "Teste@123"
        };

        Client = Factory.CreateClient();

        var response = await Client.PostAsJsonAsync("api/Auth/login", userData);
        response.EnsureSuccessStatusCode();
        UsuarioToken = await response.Content.ReadAsStringAsync();
    }
    public async Task DoLoginAsAdmin()
    {
        var userData = new LoginRequest
        {
            Email = "admin@fluencyhub.com\",",
            Password = "Teste@123"
        };

        Client = Factory.CreateClient();

        var response = await Client.PostAsJsonAsync("api/Auth/login", userData);
        response.EnsureSuccessStatusCode();
        UsuarioToken = await response.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
