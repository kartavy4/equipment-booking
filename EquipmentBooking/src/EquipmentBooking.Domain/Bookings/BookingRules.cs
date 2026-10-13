using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Bookings;

public static class BookingRules
{
    public static void Validate(DateTimeOffset start, DateTimeOffset end, DateTimeOffset now)
    {
        if (start <= now)
        {
            throw new DomainRuleException("Начало должно быть в будущем.");
        }

        if (end <= start)
        {
            throw new DomainRuleException("Окончание должно быть позже начала.");
        }

        if (end - start > TimeSpan.FromHours(24))
        {
            throw new DomainRuleException("Максимальная длительность — 24 часа.");
        }

        if (start > now.AddDays(90))
        {
            throw new DomainRuleException("Бронировать можно на 90 дней вперёд.");
        }
    }
}
