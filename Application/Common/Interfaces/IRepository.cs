
using System.Linq.Expressions;

namespace HotelManagement.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(
        int id,
        CancellationToken ct = default,
        params Expression<Func<T, object>>[] includes);

    Task<(IReadOnlyList<T> Items, int Total)> GetPagedAsync(
        Expression<Func<T, bool>>? filter,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default,
        params Expression<Func<T, object>>[] includes);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);

    

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default,
    params Expression<Func<T, object>>[] includes);

    
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}