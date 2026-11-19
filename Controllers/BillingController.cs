
using HotelManagement.Application.Billing;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]

[Route("api/reservations/{reservationId:int}")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billing;

    public BillingController(IBillingService billing) => _billing = billing;

  
    [Authorize(Roles = Roles.CustomerOrFrontDesk)]
    [HttpGet("bill")]
    public async Task<ActionResult<BillResponse>> GetBill(int reservationId, CancellationToken ct)
        => Ok(await _billing.GetBillAsync(reservationId, ct));

    [Authorize(Roles = Roles.CustomerOrFrontDesk)]
    [HttpPost("extras")]
    public async Task<ActionResult<BillResponse>> AddExtra(
        int reservationId, AddExtraRequest request, CancellationToken ct)
        => Ok(await _billing.AddExtraAsync(reservationId, request, ct));

   
    [Authorize(Roles = Roles.CustomerOrFrontDesk)]
    [HttpDelete("extras/{extraId:int}")]
    public async Task<ActionResult<BillResponse>> RemoveExtra(
        int reservationId, int extraId, CancellationToken ct)
        => Ok(await _billing.RemoveExtraAsync(reservationId, extraId, ct));

    
    [Authorize(Roles = Roles.CustomerOrFrontDesk)]
    [HttpPost("payments")]
    public async Task<ActionResult<BillResponse>> Pay(
        int reservationId, CreatePaymentRequest request, CancellationToken ct)
        => Ok(await _billing.PayAsync(reservationId, request, ct));

   
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("refunds")]
    public async Task<ActionResult<BillResponse>> Refund(
        int reservationId, RefundRequest request, CancellationToken ct)
        => Ok(await _billing.RefundAsync(reservationId, request, ct));
}