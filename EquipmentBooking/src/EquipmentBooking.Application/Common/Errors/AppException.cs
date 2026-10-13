namespace EquipmentBooking.Application.Common.Errors;

public sealed class AppException(ErrorType type, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public ErrorType Type { get; } = type;
}
