using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Bookings.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public sealed class BookingsController(IBookingService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await bookings.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBooking request, CancellationToken cancellationToken) =>
        Ok(await bookings.CreateAsync(request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await bookings.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
