// Controllers/RoomTypesController.cs
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.RoomTypes;
using Microsoft.AspNetCore.Mvc;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagement.API.Controllers;

[ApiController]                  // بيفعّل سلوكيات مفيدة للـ API (زي ربط الـ JSON تلقائي)
[Route("api/[controller]")]      // [controller] بيتحول لـ "RoomTypes" => api/roomtypes
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _service;

    public RoomTypesController(IRoomTypeService service) => _service = service;

    // GET api/roomtypes?search=suite&sortBy=price&descending=true&pageNumber=1&pageSize=10
    // [FromQuery]: بيملا الـ RoomTypeQuery من الـ query string
    [HttpGet]
    public async Task<ActionResult<PagedResult<RoomTypeResponse>>> GetAll(
        [FromQuery] RoomTypeQuery query, CancellationToken ct)
        => Ok(await _service.GetAllAsync(query, ct));

    // GET api/roomtypes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomTypeResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    
    [Authorize(Roles = Roles.AdminOrManager)]   
    [HttpPost]
    public async Task<ActionResult<RoomTypeResponse>> Create(SaveRoomTypeRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoomTypeResponse>> Update(int id, SaveRoomTypeRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}