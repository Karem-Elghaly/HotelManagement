
using HotelManagement.Application.Reports;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence.Queries;

public class ReportsQuery : IReportsQuery
{
 
    private static readonly ReservationStatus[] CountedStatuses =
    [
        ReservationStatus.Confirmed,
        ReservationStatus.CheckedIn,
        ReservationStatus.CheckedOut
    ];

    private readonly AppDbContext _db;

    public ReportsQuery(AppDbContext db) => _db = db;

    public Task<int> CountSellableRoomsAsync(CancellationToken ct = default)
        => _db.Rooms.CountAsync(r => r.Status != RoomStatus.OutOfService, ct);

    public async Task<IReadOnlyList<StayRow>> GetStaysAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
       
        var rows = await _db.Reservations
            .AsNoTracking()
            .Where(r => CountedStatuses.Contains(r.Status) && r.CheckInDate < to && r.CheckOutDate > from)
            .Select(r => new { r.CheckInDate, r.CheckOutDate, r.TotalPrice })
            .ToListAsync(ct);

        return rows.Select(r => new StayRow(r.CheckInDate, r.CheckOutDate, r.TotalPrice)).ToList();
    }

    public async Task<IReadOnlyList<PaymentAggregateRow>> GetPaymentAggregatesAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var (fromUtc, toUtc) = ToUtcRange(from, to);

  
        var rows = await _db.Payments
            .AsNoTracking()
            .Where(p => p.CreatedAt >= fromUtc && p.CreatedAt < toUtc)
            .GroupBy(p => new { Day = p.CreatedAt.Date, p.Type, p.Method })
            .Select(g => new { g.Key.Day, g.Key.Type, g.Key.Method, Amount = g.Sum(p => p.Amount) })
            .ToListAsync(ct);

        return rows
            .Select(r => new PaymentAggregateRow(DateOnly.FromDateTime(r.Day), r.Type, r.Method, r.Amount))
            .ToList();
    }

    public async Task<IReadOnlyList<ExtraAggregateRow>> GetExtrasAggregatesAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var (fromUtc, toUtc) = ToUtcRange(from, to);

      
        var rows = await _db.ReservationExtras
            .AsNoTracking()
            .Where(e => e.CreatedAt >= fromUtc && e.CreatedAt < toUtc
                        && e.Reservation.Status != ReservationStatus.Cancelled)
            .GroupBy(e => e.ServiceName)
            .Select(g => new
            {
                Name = g.Key,
                Quantity = g.Sum(e => e.Quantity),
                Revenue = g.Sum(e => e.UnitPrice * e.Quantity)
            })
            .ToListAsync(ct);

        return rows.Select(r => new ExtraAggregateRow(r.Name, r.Quantity, r.Revenue)).ToList();
    }

    public async Task<DashboardResponse> GetDashboardAsync(DateOnly today, CancellationToken ct = default)
    {
  
        var statusCounts = await _db.Rooms
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        
        var roomsByStatus = Enum.GetValues<RoomStatus>()
            .ToDictionary(s => s.ToString(), s => statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0);

        
        var arrivals = await _db.Reservations.CountAsync(
            r => r.Status == ReservationStatus.Confirmed && r.CheckInDate == today, ct);

        var departures = await _db.Reservations.CountAsync(
            r => r.Status == ReservationStatus.CheckedIn && r.CheckOutDate == today, ct);

        var inHouse = await _db.Reservations.CountAsync(r => r.Status == ReservationStatus.CheckedIn, ct);
        var pending = await _db.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending, ct);

        var openTasks = await _db.HousekeepingTasks.CountAsync(
            t => HousekeepingTask.OpenStatuses.Contains(t.Status), ct);

        var openMaintenance = await _db.MaintenanceRequests.CountAsync(
            m => MaintenanceRequest.ActiveStatuses.Contains(m.Status), ct);

        return new DashboardResponse(
            today, roomsByStatus, arrivals, departures, inHouse, pending, openTasks, openMaintenance);
    }

   
    private static (DateTime From, DateTime To) ToUtcRange(DateOnly from, DateOnly to)
        => (from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
}