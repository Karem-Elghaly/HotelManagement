
using HotelManagement.Application.Availability;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _service;

    public AvailabilityController(IAvailabilityService service) => _service = service;


    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomTypeAvailabilityResponse>>> Search(
        [FromQuery] AvailabilityQuery query, CancellationToken ct)
        => Ok(await _service.SearchAsync(query, ct));

    
    [Authorize(Roles = Roles.FrontDesk)]
    [HttpGet("rooms")]
    public async Task<ActionResult<IReadOnlyList<AvailableRoomResponse>>> GetRooms(
        [FromQuery] AvailabilityQuery query, CancellationToken ct)
        => Ok(await _service.GetAvailableRoomsAsync(query, ct));
}