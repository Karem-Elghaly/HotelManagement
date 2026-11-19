
using System.Linq.Expressions;
using HotelManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _db;
    private readonly DbSet<T> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(
        int id,
        CancellationToken ct = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set;

        foreach (var include in includes)
            query = query.Include(include);

  
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);
    }

    public async Task<(IReadOnlyList<T> Items, int Total)> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set.AsNoTracking();

        if (filter is not null)
            query = query.Where(filter);

    
        var total = await query.CountAsync(ct);

        foreach (var include in includes)
            query = query.Include(include);

        if (orderBy is not null)
            query = orderBy(query);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => _set.AnyAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set;  

        foreach (var include in includes)
            query = query.Include(include);

        return query.FirstOrDefaultAsync(predicate, ct);


    }

}