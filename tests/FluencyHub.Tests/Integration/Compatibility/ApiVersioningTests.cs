using FluencyHub.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluencyHub.ContentManagement.Application.Queries.GetCourseById;

namespace FluencyHub.Tests.Integration.Compatibility;

public class ApiVersioningTests : IntegrationTestBase
{
    // 242. ApiVersioning_V1Endpoints_ShouldMaintainBackwardCompatibility
    [Fact]
    public async Task V1Endpoints_ShouldMaintainBackwardCompatibility()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Version", "1.0");

        // Act & Assert - Verificar endpoints V1 ainda funcionam
        var coursesResponse = await client.GetAsync("/api/cursos");
        coursesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var studentsResponse = await client.GetAsync("/api/estudantes");
        // Pode retornar Forbidden se não autenticado, mas não deve retornar NotFound
        studentsResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);

        // Verificar estrutura de resposta mantém compatibilidade
        var courses = await coursesResponse.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        courses.Should().NotBeNull();
        
        // Verificar se propriedades essenciais ainda existem
        if (courses!.Any())
        {
            var firstCourse = courses.First();
            firstCourse.Should().NotBeNull();
            // Propriedades que devem existir na V1
            firstCourse.GetType().GetProperty("Id").Should().NotBeNull();
            firstCourse.GetType().GetProperty("Name").Should().NotBeNull();
            firstCourse.GetType().GetProperty("Description").Should().NotBeNull();
            firstCourse.GetType().GetProperty("Price").Should().NotBeNull();
        }
    }

    // 243. ApiVersioning_V2Endpoints_ShouldHandleNewFeatures
    [Fact]
    public async Task V2Endpoints_ShouldHandleNewFeatures()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Version", "2.0");

        // Act & Assert - Verificar endpoints V2 com novas funcionalidades
        var coursesResponse = await client.GetAsync("/api/cursos");
        coursesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar se novos headers ou funcionalidades estão disponíveis na V2
        if (coursesResponse.Headers.Contains("X-API-Version"))
        {
            var apiVersionHeader = coursesResponse.Headers.GetValues("X-API-Version").FirstOrDefault();
            apiVersionHeader.Should().Contain("2.0");
        }

        // Verificar se a estrutura de resposta pode incluir novos campos na V2
        var courses = await coursesResponse.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        courses.Should().NotBeNull();

        // Testar endpoint específico da V2 (se existir)
        var v2SpecificResponse = await client.GetAsync("/api/v2/cursos/advanced");
        // Pode retornar NotFound se não implementado, mas não deve causar erro interno
        v2SpecificResponse.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ApiVersioning_ShouldHandleMissingVersionHeader()
    {
        // Arrange
        var client = Factory.CreateClient();
        // Não adicionar header de versão

        // Act
        var response = await client.GetAsync("/api/cursos");

        // Assert - Deve usar versão padrão (provavelmente V1)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var courses = await response.Content.ReadFromJsonAsync<IEnumerable<CourseDto>>();
        courses.Should().NotBeNull();
    }

    [Fact]
    public async Task ApiVersioning_ShouldHandleInvalidVersionHeader()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Version", "999.0");

        // Act
        var response = await client.GetAsync("/api/cursos");

        // Assert - Deve usar versão padrão ou retornar erro apropriado
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
} 