using EquipmentBooking.Application.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Equipment.Contracts;
using EquipmentBooking.Domain.Equipment;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Equipment;

internal sealed class EquipmentService(IApplicationDbContext db, TimeProvider clock) : IEquipmentService
{
    public async Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Equipment.AsNoTracking().OrderBy(item => item.Name)
            .Select(item => new EquipmentResponse(item.Id, item.Name, item.Category, item.Description, item.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<EquipmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await FindAsync(id, cancellationToken);
        return ToResponse(item);
    }

    public async Task<IReadOnlyList<ScheduleSlotResponse>> GetScheduleAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await db.Equipment.AnyAsync(item => item.Id == id, cancellationToken))
        {
            throw NotFound();
        }

        var now = clock.GetUtcNow().UtcDateTime;
        return await db.Bookings.AsNoTracking()
            .Where(booking => booking.EquipmentId == id && !booking.IsCancelled && booking.EndsAt > now)
            .OrderBy(booking => booking.StartsAt)
            .Select(booking => new ScheduleSlotResponse(booking.StartsAt, booking.EndsAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<EquipmentResponse> CreateAsync(EquipmentRequest request, CancellationToken cancellationToken)
    {
        var item = new EquipmentItem(request.Name, request.Category, request.Description);
        db.Equipment.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(item);
    }

    public async Task<EquipmentResponse> UpdateAsync(Guid id, EquipmentRequest request, CancellationToken cancellationToken)
    {
        var item = await FindAsync(id, cancellationToken);
        item.Update(request.Name, request.Category, request.Description, request.IsActive);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(item);
    }

    private async Task<EquipmentItem> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Equipment.SingleOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw NotFound();

    private static AppException NotFound() => new(ErrorType.NotFound, "Оборудование не найдено.");

    private static EquipmentResponse ToResponse(EquipmentItem item) =>
        new(item.Id, item.Name, item.Category, item.Description, item.IsActive);
}
