using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EquipmentBooking.Application.Auth.Contracts;
using EquipmentBooking.Application.Bookings.Contracts;
using EquipmentBooking.Application.Equipment.Contracts;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EquipmentBooking.Tests;

public class BookingTests
{
    [Fact]
    public async Task BookingLifecycleAndPermissions()
    {
        using var factory = new ApiFactory();
        using var alice = factory.CreateClient();
        using var bob = factory.CreateClient();
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/bookings")).StatusCode);
        // A state-changing request without CSRF must fail, even before login.
        Assert.Equal(HttpStatusCode.BadRequest, (await anonymous.PostAsJsonAsync("/api/auth/register", new { name = "Test", email = "bad@example.com", password = "Password123!" })).StatusCode);
        await Register(alice, "alice@example.com"); await Register(bob, "bob@example.com");
        var equipment = (await alice.GetFromJsonAsync<JsonElement>("/api/equipment"))[0].GetProperty("id").GetGuid();
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var request = new CreateBooking(equipment, start, start.AddHours(1));
        var created = await Post(alice, "/api/bookings", request);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await Post(bob, "/api/bookings", request)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Post(bob, "/api/bookings", request with { StartsAt = start.AddHours(1), EndsAt = start.AddHours(2) })).StatusCode);
        var bobsBookings = await bob.GetFromJsonAsync<BookingView[]>("/api/bookings");
        Assert.DoesNotContain(bobsBookings!, x => x.Id == id);
        Assert.Equal(HttpStatusCode.NotFound, (await Post(bob, $"/api/bookings/{id}/cancel", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Post(bob, "/api/equipment", new EquipmentRequest("Name", "Category", "Description"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Post(alice, $"/api/bookings/{id}/cancel", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Post(bob, "/api/bookings", request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Post(alice, "/api/bookings", request with { EndsAt = start })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await Post(alice, "/api/auth/logout", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await alice.GetAsync("/api/bookings")).StatusCode);
    }

    [Fact]
    public async Task AdminCanAddAndDisableEquipment()
    {
        using var factory = new ApiFactory(); using var admin = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await Post(admin, "/api/auth/login", new LoginRequest("admin@test.local", "AdminPassword123!"))).StatusCode);
        var response = await Post(admin, "/api/equipment", new EquipmentRequest("Test camera", "Камеры", "Test"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var item = (await response.Content.ReadFromJsonAsync<EquipmentResponse>())!;
        await SetToken(admin);
        Assert.Equal(HttpStatusCode.OK, (await admin.PutAsJsonAsync($"/api/equipment/{item.Id}", new EquipmentRequest(item.Name, item.Category, item.Description, false))).StatusCode);
        var start = DateTimeOffset.UtcNow.AddDays(1);
        Assert.Equal(HttpStatusCode.Conflict, (await Post(admin, "/api/bookings", new CreateBooking(item.Id, start, start.AddHours(1)))).StatusCode);
    }
    private record BookingView(Guid Id);
    private static async Task Register(HttpClient c, string email) => Assert.Equal(HttpStatusCode.OK,
        (await Post(c, "/api/auth/register", new RegisterRequest("Test member", email, "Password123!"))).StatusCode);
    internal static async Task SetToken(HttpClient c)
    {
        var csrf = await c.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        c.DefaultRequestHeaders.Remove("X-CSRF-TOKEN"); c.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
    }
    internal static async Task<HttpResponseMessage> Post(HttpClient c, string path, object body)
    { await SetToken(c); return await c.PostAsJsonAsync(path, body); }
}
