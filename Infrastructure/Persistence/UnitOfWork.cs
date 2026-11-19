
using HotelManagement.Application.Common.Interfaces;

namespace HotelManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
    
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        var result = await action();

        await tx.CommitAsync(ct);   
        return result;
    }
}