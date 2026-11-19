using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Reservations;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service) => _service = service;

   
    [Authorize(Roles = Roles.CustomerOrFrontDesk)]
    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReservationResponse>>> GetAll(
        [FromQuery] ReservationQuery query, CancellationToken ct)
        => Ok(await _service.GetAllAsync(query, ct));


    [Authorize(Roles = Roles.Customer)]
    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<ReservationResponse>>> GetMine(
        [FromQuery] ReservationQuery query, CancellationToken ct)
        => Ok(await _service.GetMineAsync(query, ct));

   
    [Authorize(Roles = Roles.CustomerOrFrontDesk)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

  
    [Authorize(Roles = Roles.FrontDesk)]
    [HttpGet("code/{code}")]
    public async Task<ActionResult<ReservationResponse>> GetByCode(string code, CancellationToken ct)
        => Ok(await _service.GetByCodeAsync(code, ct));

   

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<ReservationResponse>> Confirm(int id, CancellationToken ct)
        => Ok(await _service.ConfirmAsync(id, ct));

    
    [Authorize(Roles = Roles.CustomerOrFrontDesk)]
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ReservationResponse>> Cancel(int id, CancellationToken ct)
        => Ok(await _service.CancelAsync(id, ct));

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost("{id:int}/check-in")]
    public async Task<ActionResult<ReservationResponse>> CheckIn(int id, CancellationToken ct)
        => Ok(await _service.CheckInAsync(id, ct));

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost("{id:int}/check-out")]
    public async Task<ActionResult<ReservationResponse>> CheckOut(int id, CancellationToken ct)
        => Ok(await _service.CheckOutAsync(id, ct));

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost("{id:int}/no-show")]
    public async Task<ActionResult<ReservationResponse>> NoShow(int id, CancellationToken ct)
        => Ok(await _service.MarkNoShowAsync(id, ct));
}