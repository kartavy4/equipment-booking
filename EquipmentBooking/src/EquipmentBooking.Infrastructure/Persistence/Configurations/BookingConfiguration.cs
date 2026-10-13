using EquipmentBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBooking.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(booking => booking.Id);
        builder.HasIndex(booking => new { booking.EquipmentId, booking.StartsAt, booking.EndsAt });
        builder.HasOne(booking => booking.User).WithMany()
            .HasForeignKey(booking => booking.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(booking => booking.Equipment).WithMany()
            .HasForeignKey(booking => booking.EquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
