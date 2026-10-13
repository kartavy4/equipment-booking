using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBooking.Infrastructure.Persistence;

// Schema generation always targets PostgreSQL; demo settings must not affect migrations.
public sealed class BookingDbContextFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql("Host=localhost;Database=equipment;Username=equipment;Password=design-time-only")
            .Options;

        return new BookingDbContext(options);
    }
}
