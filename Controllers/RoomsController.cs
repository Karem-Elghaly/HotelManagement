
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Rooms;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Staff)]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _service;

    public RoomsController(IRoomService service) => _service = service;

   
    [HttpGet]
    public async Task<ActionResult<PagedResult<RoomResponse>>> GetAll(
        [FromQuery] RoomQuery query, CancellationToken ct)
        => Ok(await _service.GetAllAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost]
    public async Task<ActionResult<RoomResponse>> Create(SaveRoomRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoomResponse>> Update(int id, SaveRoomRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<RoomResponse>> UpdateStatus(
        int id, UpdateRoomStatusRequest request, CancellationToken ct)
        => Ok(await _service.UpdateStatusAsync(id, request, ct));
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}