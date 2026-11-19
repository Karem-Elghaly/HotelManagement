
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Maintenance;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/maintenance-requests")]
[Authorize(Roles = Roles.Staff)]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _service;

    public MaintenanceController(IMaintenanceService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MaintenanceResponse>>> GetAll(
        [FromQuery] MaintenanceQuery query, CancellationToken ct)
        => Ok(await _service.GetAllAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaintenanceResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

   
    [HttpPost]
    public async Task<ActionResult<MaintenanceResponse>> Create(CreateMaintenanceRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<MaintenanceResponse>> Start(int id, CancellationToken ct)
        => Ok(await _service.StartAsync(id, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("{id:int}/resolve")]
    public async Task<ActionResult<MaintenanceResponse>> Resolve(
        int id, ResolveMaintenanceRequest request, CancellationToken ct)
        => Ok(await _service.ResolveAsync(id, request, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<MaintenanceResponse>> Cancel(int id, CancellationToken ct)
        => Ok(await _service.CancelAsync(id, ct));
}