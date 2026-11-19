
using HotelManagement.Application.Reports;
using HotelManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    [Authorize(Roles = Roles.FrontDesk)]
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> Dashboard(CancellationToken ct)
        => Ok(await _service.GetDashboardAsync(ct));

    
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpGet("occupancy")]
    public async Task<ActionResult<OccupancyReport>> Occupancy([FromQuery] ReportRangeQuery query, CancellationToken ct)
        => Ok(await _service.GetOccupancyAsync(query, ct));

    
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReport>> Revenue([FromQuery] ReportRangeQuery query, CancellationToken ct)
        => Ok(await _service.GetRevenueAsync(query, ct));
}