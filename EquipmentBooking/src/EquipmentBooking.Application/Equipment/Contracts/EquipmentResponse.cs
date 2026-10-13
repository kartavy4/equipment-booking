namespace EquipmentBooking.Application.Equipment.Contracts;

public sealed record EquipmentResponse(Guid Id, string Name, string Category, string Description, bool IsActive);
