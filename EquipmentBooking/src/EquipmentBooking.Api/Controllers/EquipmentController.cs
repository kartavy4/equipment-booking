using EquipmentBooking.Application.Equipment;
using EquipmentBooking.Application.Equipment.Contracts;
using EquipmentBooking.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

[ApiController]
[Route("api/equipment")]
public sealed class EquipmentController(IEquipmentService equipment) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EquipmentResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await equipment.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EquipmentResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await equipment.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:guid}/schedule")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ScheduleSlotResponse>>> GetSchedule(Guid id, CancellationToken cancellationToken) =>
        Ok(await equipment.GetScheduleAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EquipmentResponse>> Create(EquipmentRequest request, CancellationToken cancellationToken)
    {
        var item = await equipment.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EquipmentResponse>> Update(Guid id, EquipmentRequest request, CancellationToken cancellationToken) =>
        Ok(await equipment.UpdateAsync(id, request, cancellationToken));
}
