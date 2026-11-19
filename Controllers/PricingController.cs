
using HotelManagement.Application.Pricing;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
public class PricingController : ControllerBase
{
    private readonly IPricingService _pricing;
    private readonly ISeasonalRateService _rates;

    public PricingController(IPricingService pricing, ISeasonalRateService rates)
    {
        _pricing = pricing;
        _rates = rates;
    }

    [HttpGet("api/pricing/quote")]
    public async Task<ActionResult<PriceQuote>> Quote([FromQuery] QuoteQuery query, CancellationToken ct)
        => Ok(await _pricing.GetQuoteAsync(query, ct));



    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpGet("api/seasonal-rates")]
    public async Task<ActionResult<IReadOnlyList<SeasonalRateResponse>>> GetAll(CancellationToken ct)
        => Ok(await _rates.GetAllAsync(ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpGet("api/seasonal-rates/{id:int}")]
    public async Task<ActionResult<SeasonalRateResponse>> GetById(int id, CancellationToken ct)
        => Ok(await _rates.GetByIdAsync(id, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("api/seasonal-rates")]
    public async Task<ActionResult<SeasonalRateResponse>> Create(SaveSeasonalRateRequest request, CancellationToken ct)
    {
        var created = await _rates.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPut("api/seasonal-rates/{id:int}")]
    public async Task<ActionResult<SeasonalRateResponse>> Update(int id, SaveSeasonalRateRequest request, CancellationToken ct)
        => Ok(await _rates.UpdateAsync(id, request, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpDelete("api/seasonal-rates/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _rates.DeleteAsync(id, ct);
        return NoContent();
    }
}