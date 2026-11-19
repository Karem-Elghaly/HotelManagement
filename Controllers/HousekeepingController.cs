
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Housekeeping;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/housekeeping-tasks")]
[Authorize(Roles = Roles.Staff)]   
public class HousekeepingController : ControllerBase
{
    private readonly IHousekeepingService _service;

    public HousekeepingController(IHousekeepingService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<HousekeepingTaskResponse>>> GetAll(
        [FromQuery] HousekeepingTaskQuery query, CancellationToken ct)
        => Ok(await _service.GetAllAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HousekeepingTaskResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));


    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost]
    public async Task<ActionResult<HousekeepingTaskResponse>> Create(
        CreateHousekeepingTaskRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    
    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<HousekeepingTaskResponse>> Start(int id, CancellationToken ct)
        => Ok(await _service.StartAsync(id, ct));

    
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<HousekeepingTaskResponse>> Complete(
        int id, CompleteHousekeepingTaskRequest request, CancellationToken ct)
        => Ok(await _service.CompleteAsync(id, request, ct));

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<HousekeepingTaskResponse>> Cancel(int id, CancellationToken ct)
        => Ok(await _service.CancelAsync(id, ct));
}