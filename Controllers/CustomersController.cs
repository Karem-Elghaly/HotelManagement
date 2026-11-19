
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Customers;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service) => _service = service;

   
    [Authorize(Roles = Roles.Customer)]
    [HttpGet("me")]
    public async Task<ActionResult<CustomerResponse>> GetMine(CancellationToken ct)
        => Ok(await _service.GetMineAsync(ct));

   
    [Authorize(Roles = Roles.Customer)]
    [HttpPut("me")]
    public async Task<ActionResult<CustomerResponse>> UpdateMine(SaveCustomerRequest request, CancellationToken ct)
        => Ok(await _service.UpdateMineAsync(request, ct));

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerResponse>>> GetAll(
        [FromQuery] CustomerQuery query, CancellationToken ct)
        => Ok(await _service.GetAllAsync(query, ct));


    [Authorize(Roles = Roles.FrontDesk)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));


    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(SaveCustomerRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> Update(int id, SaveCustomerRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}