using EquipmentBooking.Application.Bookings.Contracts;

namespace EquipmentBooking.Application.Bookings;

public interface IBookingService
{
    Task<IReadOnlyList<BookingResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<BookingResponse> CreateAsync(CreateBooking request, CancellationToken cancellationToken);
    Task CancelAsync(Guid id, CancellationToken cancellationToken);
}
