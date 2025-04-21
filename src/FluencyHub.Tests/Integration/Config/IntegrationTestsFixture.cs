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
using FluencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.Tests.Integration.Config;

[CollectionDefinition(nameof(IntegrationWebTestsFixtureCollection))]
public class IntegrationWebTestsFixtureCollection : ICollectionFixture<IntegrationTestsFixture<Program>> { }

[CollectionDefinition(nameof(IntegrationApiTestsFixtureCollection))]
public class IntegrationApiTestsFixtureCollection : ICollectionFixture<IntegrationTestsFixture<Program>> { }

public class IntegrationTestsFixture<TProgram> : IntegrationTestsBase<TProgram> where TProgram : class
{
    public string AntiForgeryFieldName = "__RequestVerificationToken";
    public string UsuarioEmail;
    public string UsuarioSenha;

    public IntegrationTestsFixture() : base(new FluencyHubAppFactory<TProgram>())
    {
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            BaseAddress = new Uri("https://localhost:7152"),
            HandleCookies = true,
            MaxAutomaticRedirections = 7
        };

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

        await GetTokenAsync(userData.Email, userData.Password);
        SetBearerToken();
    }

    public async Task DoLoginAsAdmin()
    {
        var userData = new LoginRequest
        {
            Email = "admin@fluencyhub.com",
            Password = "Teste@123"
        };

        await GetTokenAsync(userData.Email, userData.Password);
        SetBearerToken();
    }

    protected override async Task SeedTestDataAsync(FluencyHubDbContext dbContext)
    {
        // Seed roles
        using var scope = Factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        if (!await roleManager.RoleExistsAsync("Administrator"))
        {
            await roleManager.CreateAsync(new IdentityRole("Administrator"));
        }
        if (!await roleManager.RoleExistsAsync("Student"))
        {
            await roleManager.CreateAsync(new IdentityRole("Student"));
        }

        // Seed admin user
        var adminEmail = "admin@fluencyhub.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Test@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }

        // Seed test student
        var studentEmail = "jack.student1@fluencyhub.com";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        if (studentUser == null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FirstName = "Jack",
                LastName = "Student",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(studentUser, "Test@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }
    }
}
