using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Tests.Domain;

public sealed class BookingDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, 1)]
    [InlineData(1, 26)]
    [InlineData(2200, 2201)]
    public void RejectsInvalidIntervals(int start, int end)
    {
        Assert.Throws<DomainRuleException>(() =>
            BookingRules.Validate(Now.AddHours(start), Now.AddHours(end), Now));
    }

    [Fact]
    public void AcceptsExactly24Hours()
    {
        BookingRules.Validate(Now.AddHours(1), Now.AddHours(25), Now);
    }

    [Fact]
    public void StoresUtcAndCancellationIsIdempotent()
    {
        var start = Now.AddHours(2).ToOffset(TimeSpan.FromHours(5));
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), start, start.AddHours(1), Now);

        Assert.Equal(DateTimeKind.Utc, booking.StartsAt.Kind);
        Assert.Equal(start.UtcDateTime, booking.StartsAt);

        booking.Cancel(Now);
        booking.Cancel(Now.AddDays(1));

        Assert.True(booking.IsCancelled);
    }

    [Fact]
    public void CompletedBookingCannotBeCancelled()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), Now.AddHours(1), Now.AddHours(2), Now);

        Assert.Throws<DomainRuleException>(() => booking.Cancel(Now.AddHours(3)));
    }
}
