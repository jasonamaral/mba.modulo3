using FluencyHub.Application.StudentManagement.Commands.CreateStudent;
using FluencyHub.Tests.Integration.Config;
using System.Net.Http.Json;

namespace FluencyHub.Tests.Integration;

[TestCaseOrderer("Features.Tests.PriorityOrderer", "Features.Tests")]
[Collection(nameof(IntegrationWebTestsFixtureCollection))]
public class StudentTests
{
    private readonly IntegrationTestsFixture<Program> _testsFixture;

    public StudentTests(IntegrationTestsFixture<Program> testsFixture)
    {
        _testsFixture = testsFixture;
    }

    [Fact(DisplayName = "Successfully register"), TestPriority(100)]
    [Trait("Category", "Student")]
    public async Task RegisterStudent_ShouldReturnSuccess()
    {
        try
        {
            // Arrange
            _testsFixture.Client.JsonMediaType();

            var command = new CreateStudentCommand
            {
                DateOfBirth = DateTime.Today.AddYears(-18),
                Email = $"jack.student1@fluencyhub.com",
                FirstName = "Jack",
                LastName = "Student",
                PhoneNumber = "11912345678",
                Password = "Test@123"
            };

            // Act
            var postResponse = await _testsFixture.Client.PostAsJsonAsync("api/students", command);

            // Assert
            postResponse.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new Exception($"Test failed. Full error details: {ex}", ex);
        }
    }

}