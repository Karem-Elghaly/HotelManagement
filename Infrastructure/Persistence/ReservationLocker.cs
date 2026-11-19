
using HotelManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence;

public class ReservationLocker : IReservationLocker
{
    private readonly AppDbContext _db;

    public ReservationLocker(AppDbContext db) => _db = db;

    public async Task LockAsync(int reservationId, CancellationToken ct = default)
    {
    
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT Id FROM Reservations WITH (UPDLOCK, ROWLOCK) WHERE Id = {reservationId}", ct);
    }
}