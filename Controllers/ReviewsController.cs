
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Reviews;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(IReviewService service) => _service = service;

   
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReviewResponse>>> GetPublic(
        [FromQuery] ReviewQuery query, CancellationToken ct)
        => Ok(await _service.GetPublicAsync(query, ct));

   
    [HttpGet("summary")]
    public async Task<ActionResult<ReviewSummaryResponse>> GetSummary(CancellationToken ct)
        => Ok(await _service.GetSummaryAsync(ct));

  
    [Authorize(Roles = Roles.Customer)]
    [HttpPost("~/api/reservations/{reservationId:int}/review")]
    public async Task<ActionResult<ReviewResponse>> Create(
        int reservationId, SaveReviewRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _service.CreateAsync(reservationId, request, ct));

   
    [Authorize(Roles = Roles.Customer)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReviewResponse>> Update(int id, SaveReviewRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpGet("manage")]
    public async Task<ActionResult<PagedResult<StaffReviewResponse>>> GetForStaff(
        [FromQuery] ReviewQuery query, CancellationToken ct)
        => Ok(await _service.GetForStaffAsync(query, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("{id:int}/reply")]
    public async Task<ActionResult<StaffReviewResponse>> Reply(
        int id, ReplyToReviewRequest request, CancellationToken ct)
        => Ok(await _service.ReplyAsync(id, request, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("{id:int}/hide")]
    public async Task<ActionResult<StaffReviewResponse>> Hide(int id, CancellationToken ct)
        => Ok(await _service.SetHiddenAsync(id, hidden: true, ct));

    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("{id:int}/show")]
    public async Task<ActionResult<StaffReviewResponse>> Show(int id, CancellationToken ct)
        => Ok(await _service.SetHiddenAsync(id, hidden: false, ct));
}