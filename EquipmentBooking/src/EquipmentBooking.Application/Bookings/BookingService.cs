using EquipmentBooking.Application.Abstractions;
using EquipmentBooking.Application.Bookings.Contracts;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Bookings;

internal sealed class BookingService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    TimeProvider clock) : IBookingService
{
    public async Task<IReadOnlyList<BookingResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var isAdmin = currentUser.IsAdmin;

        return await db.Bookings.AsNoTracking()
            .Where(booking => isAdmin || booking.UserId == userId)
            .OrderByDescending(booking => booking.StartsAt)
            .Select(booking => new BookingResponse(
                booking.Id, booking.EquipmentId, booking.Equipment.Name, booking.User.Name,
                booking.StartsAt, booking.EndsAt, booking.IsCancelled))
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingResponse> CreateAsync(CreateBooking request, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var booking = new Booking(request.EquipmentId, currentUser.Id, request.StartsAt, request.EndsAt, now);
        var equipment = await db.Equipment.SingleOrDefaultAsync(item => item.Id == request.EquipmentId, cancellationToken)
            ?? throw new AppException(ErrorType.NotFound, "Оборудование не найдено.");

        if (!equipment.IsActive)
        {
            throw new AppException(ErrorType.Conflict, "Оборудование временно недоступно.");
        }

        if (await db.Bookings.AnyAsync(existing =>
                existing.EquipmentId == booking.EquipmentId && !existing.IsCancelled &&
                existing.StartsAt < booking.EndsAt && existing.EndsAt > booking.StartsAt, cancellationToken))
        {
            throw new AppException(ErrorType.Conflict, "Это время уже занято. Выберите другой интервал.");
        }

        var userName = await db.Users.Where(user => user.Id == booking.UserId)
            .Select(user => user.Name).SingleAsync(cancellationToken);

        db.Bookings.Add(booking);
        // A single SaveChanges is atomic; the database constraint also handles racing requests.
        await db.SaveChangesAsync(cancellationToken);

        return new BookingResponse(
            booking.Id, equipment.Id, equipment.Name, userName,
            booking.StartsAt, booking.EndsAt, booking.IsCancelled);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var isAdmin = currentUser.IsAdmin;
        var booking = await db.Bookings.SingleOrDefaultAsync(
            booking => booking.Id == id && (booking.UserId == userId || isAdmin), cancellationToken)
            ?? throw new AppException(ErrorType.NotFound, "Бронирование не найдено.");

        if (!booking.IsCancelled && booking.EndsAt <= clock.GetUtcNow().UtcDateTime)
        {
            throw new AppException(ErrorType.Conflict, "Завершённое бронирование нельзя отменить.");
        }

        booking.Cancel(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }
}
