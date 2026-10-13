using System.Net;
using System.Net.Http.Json;
using EquipmentBooking.Application.Auth.Contracts;
using EquipmentBooking.Application.Bookings.Contracts;
using EquipmentBooking.Application.Equipment.Contracts;
using System.Text.Json;

namespace EquipmentBooking.Tests;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_POSTGRES")))
        {
            Skip = "Set TEST_POSTGRES to a dedicated test database connection string (see README).";
        }
    }
}
public class PostgresTests
{
    [PostgresFact]
    public async Task ConcurrentRequestsCannotDoubleBook()
    {
        using var factory = new ApiFactory(Environment.GetEnvironmentVariable("TEST_POSTGRES"));
        using var alice = factory.CreateClient(); using var bob = factory.CreateClient();
        await BookingTests.Post(alice, "/api/auth/login", new LoginRequest("admin@test.local", "AdminPassword123!"));
        var created = await BookingTests.Post(alice, "/api/equipment", new EquipmentRequest($"Concurrency {Guid.NewGuid()}", "Test", "Two concurrent requests"));
        created.EnsureSuccessStatusCode();
        var equipment = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        (await BookingTests.Post(bob, "/api/auth/register", new RegisterRequest("Concurrent member", $"{Guid.NewGuid()}@test.local", "Password123!"))).EnsureSuccessStatusCode();
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var booking = new CreateBooking(equipment, start, start.AddHours(1));
        await BookingTests.SetToken(alice); await BookingTests.SetToken(bob);
        var results = await Task.WhenAll(alice.PostAsJsonAsync("/api/bookings", booking), bob.PostAsJsonAsync("/api/bookings", booking));
        Assert.Single(results, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(results, r => r.StatusCode == HttpStatusCode.Conflict);
    }
}
