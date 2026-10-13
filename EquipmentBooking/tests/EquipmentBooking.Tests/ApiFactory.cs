using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EquipmentBooking.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string? postgres = null;
    public ApiFactory(string? postgresConnection = null) { postgres = postgresConnection; }
    private readonly string database = Path.Combine(Path.GetTempPath(), $"equipment-test-{Guid.NewGuid()}.db");
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing").ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Database:Provider"] = postgres is null ? "Sqlite" : "Postgres",
        ["ConnectionStrings:Database"] = postgres ?? $"Data Source={database};Pooling=False",
        ["Seed:AdminEmail"] = "admin@test.local",
        ["Seed:AdminPassword"] = "AdminPassword123!",
        ["Logging:LogLevel:Default"] = "Warning"
    }));
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); if (disposing && File.Exists(database))
        {
            File.Delete(database);
        }
    }
}
