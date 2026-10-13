using EquipmentBooking.Application.Equipment.Contracts;

namespace EquipmentBooking.Application.Equipment;

public interface IEquipmentService
{
    Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<EquipmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ScheduleSlotResponse>> GetScheduleAsync(Guid id, CancellationToken cancellationToken);
    Task<EquipmentResponse> CreateAsync(EquipmentRequest request, CancellationToken cancellationToken);
    Task<EquipmentResponse> UpdateAsync(Guid id, EquipmentRequest request, CancellationToken cancellationToken);
}
