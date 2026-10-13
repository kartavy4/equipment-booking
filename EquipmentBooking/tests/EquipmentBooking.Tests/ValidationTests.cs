using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EquipmentBooking.Application.Auth.Contracts;

namespace EquipmentBooking.Tests;

public sealed class ValidationTests
{
    [Theory]
    [InlineData(null, "member@example.com", "Password123!")]
    [InlineData("  ", "member@example.com", "Password123!")]
    [InlineData("Member", "not-an-email", "Password123!")]
    [InlineData("Member", "member@example.com", "short")]
    [InlineData("Member", null, "Password123!")]
    [InlineData("Member", "member@example.com", null)]
    public async Task InvalidRegistrationReturnsCompatibleProblemDetails(string? name, string? email, string? password)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await BookingTests.Post(client, "/api/auth/register", new { name, email, password });
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("error").GetString()));
        Assert.True(problem.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task DuplicateEmailReturnsConflictAndInvalidLoginReturnsUnauthorized()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        using var other = factory.CreateClient();

        var registration = new RegisterRequest("Member", "member@example.com", "Password123!");
        (await BookingTests.Post(client, "/api/auth/register", registration)).EnsureSuccessStatusCode();

        var duplicate = await BookingTests.Post(other, "/api/auth/register",
            registration with { Email = "MEMBER@example.com" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var login = await BookingTests.Post(other, "/api/auth/login",
            new LoginRequest(registration.Email, "WrongPassword123!"));
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task ControllersExposeHealthAndEquipmentLocation()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var health = await client.GetFromJsonAsync<JsonElement>("/health");
        Assert.Equal("healthy", health.GetProperty("status").GetString());

        var equipment = (await client.GetFromJsonAsync<JsonElement>("/api/equipment"))[0];
        var id = equipment.GetProperty("id").GetGuid();
        var response = await client.GetFromJsonAsync<JsonElement>($"/api/equipment/{id}");
        Assert.Equal(id, response.GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/equipment/{Guid.NewGuid()}/schedule")).StatusCode);
    }
}
