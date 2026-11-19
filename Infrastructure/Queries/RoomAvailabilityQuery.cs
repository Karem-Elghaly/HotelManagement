// Persistence/Queries/RoomAvailabilityQuery.cs
using HotelManagement.Application.Availability;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence.Queries;

public class RoomAvailabilityQuery : IRoomAvailabilityQuery
{
    private readonly AppDbContext _db;

    public RoomAvailabilityQuery(AppDbContext db) => _db = db;

 
    private static readonly ReservationStatus[] BlockingStatuses =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed,
        ReservationStatus.CheckedIn
    ];


    private IQueryable<Reservation> OverlappingReservations(DateOnly from, DateOnly until)
        => _db.Reservations.Where(res =>
            BlockingStatuses.Contains(res.Status) &&
            res.CheckInDate < until &&
            res.CheckOutDate > from);

   
    private IQueryable<MaintenanceRequest> BlockingMaintenance(DateOnly from, DateOnly until)
        => _db.MaintenanceRequests.Where(m =>
            m.BlocksRoom &&
            MaintenanceRequest.ActiveStatuses.Contains(m.Status) &&
            m.BlockedFrom < until &&
            m.BlockedUntil > from);

    public async Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
        DateOnly checkIn, DateOnly checkOut, int adults, int children,
        int? roomTypeId, CancellationToken ct = default)
    {
        var reservations = OverlappingReservations(checkIn, checkOut);
        var maintenance = BlockingMaintenance(checkIn, checkOut);

        return await _db.Rooms
            .AsNoTracking()
            .Include(r => r.RoomType)
            .Where(r => r.Status != RoomStatus.OutOfService)
            .Where(r => r.RoomType.MaxAdults >= adults && r.RoomType.MaxChildren >= children)
            .Where(r => roomTypeId == null || r.RoomTypeId == roomTypeId)
           
            .Where(r => !reservations.Any(res => res.RoomId == r.Id))
            .Where(r => !maintenance.Any(m => m.RoomId == r.Id))
            .OrderBy(r => r.RoomType.BasePricePerNight)
            .ThenBy(r => r.Number)
            .ToListAsync(ct);
    }

    public async Task<bool> IsRoomAvailableAsync(
        int roomId, DateOnly checkIn, DateOnly checkOut,
        int? excludeReservationId = null, CancellationToken ct = default)
    {
        var conflicts = OverlappingReservations(checkIn, checkOut).Where(res => res.RoomId == roomId);

        if (excludeReservationId is not null)
            conflicts = conflicts.Where(res => res.Id != excludeReservationId);

        if (await conflicts.AnyAsync(ct))
            return false;

        return !await BlockingMaintenance(checkIn, checkOut).AnyAsync(m => m.RoomId == roomId, ct);
    }

    
    public Task<bool> HasReservationsAsync(int roomId, DateOnly from, DateOnly until, CancellationToken ct = default)
        => OverlappingReservations(from, until).AnyAsync(res => res.RoomId == roomId, ct);

    public async Task LockRoomAsync(int roomId, CancellationToken ct = default)
    {
       
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT Id FROM Rooms WITH (UPDLOCK, ROWLOCK) WHERE Id = {roomId}", ct);
    }
}