
using HotelManagement.Application.ExtraServices;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/extra-services")]
public class ExtraServicesController : ControllerBase
{
    private readonly IExtraCatalogService _service;

    public ExtraServicesController(IExtraCatalogService service) => _service = service;

    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExtraServiceResponse>>> GetActive(CancellationToken ct)
        => Ok(await _service.GetAllAsync(includeInactive: false, ct));

  
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<ExtraServiceResponse>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(includeInactive: true, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExtraServiceResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost]
    public async Task<ActionResult<ExtraServiceResponse>> Create(SaveExtraServiceRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExtraServiceResponse>> Update(int id, SaveExtraServiceRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}