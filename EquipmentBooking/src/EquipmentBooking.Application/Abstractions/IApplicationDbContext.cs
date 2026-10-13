using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Equipment;
using EquipmentBooking.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Abstractions;

// Application uses EF query abstractions, but does not depend on a concrete database/provider.
public interface IApplicationDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<EquipmentItem> Equipment { get; }
    DbSet<Booking> Bookings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
