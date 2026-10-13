using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Equipment;
using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.Domain.Bookings;

public sealed class Booking
{
    private Booking() { }

    public Booking(Guid equipmentId, Guid userId, DateTimeOffset start, DateTimeOffset end, DateTimeOffset now)
    {
        BookingRules.Validate(start, end, now);
        Id = Guid.NewGuid();
        EquipmentId = equipmentId;
        UserId = userId;
        StartsAt = start.UtcDateTime;
        EndsAt = end.UtcDateTime;
    }

    public Guid Id { get; private set; }
    public Guid EquipmentId { get; private set; }
    public EquipmentItem Equipment { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public bool IsCancelled { get; private set; }

    public void Cancel(DateTimeOffset now)
    {
        if (IsCancelled)
        {
            return;
        }

        if (EndsAt <= now.UtcDateTime)
        {
            throw new DomainRuleException("Завершённое бронирование нельзя отменить.");
        }

        IsCancelled = true;
    }
}
