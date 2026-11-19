// Customers/CustomerService.cs
using System.Linq.Expressions;
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customers;
    private readonly IRepository<Reservation> _reservations;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<SaveCustomerRequest> _validator;

    public CustomerService(
        IRepository<Customer> customers,
        IRepository<Reservation> reservations,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IValidator<SaveCustomerRequest> validator)
    {
        _customers = customers;
        _reservations = reservations;
        _uow = uow;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<PagedResult<CustomerResponse>> GetAllAsync(CustomerQuery query, CancellationToken ct = default)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var term = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

       
        Expression<Func<Customer, bool>> filter = c =>
            term == null ||
            c.FirstName.Contains(term) ||
            c.LastName.Contains(term) ||
            c.Email.Contains(term) ||
            (c.Phone != null && c.Phone.Contains(term));

        var (items, total) = await _customers.GetPagedAsync(
            filter, GetOrderBy(query), pageNumber, pageSize, ct);

        return new PagedResult<CustomerResponse>(
            items.Select(e => e.ToResponse()).ToList(), total, pageNumber, pageSize);
    }

    public async Task<CustomerResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await GetOrThrowAsync(id, ct)).ToResponse();

    public async Task<CustomerResponse> CreateAsync(SaveCustomerRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

   
        var entity = request.ToEntity();
        await _customers.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return entity.ToResponse();
    }

    public async Task<CustomerResponse> UpdateAsync(int id, SaveCustomerRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var entity = await GetOrThrowAsync(id, ct);
        request.ApplyTo(entity);

        _customers.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return entity.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetOrThrowAsync(id, ct);

       
        var hasReservations = await _reservations.ExistsAsync(r => r.CustomerId == id, ct);

        if (hasReservations)
            throw new DomainException("This customer has reservations history and cannot be deleted.");
        _customers.Remove(entity);   
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<CustomerResponse> GetMineAsync(CancellationToken ct = default)
        => (await GetMineOrThrowAsync(ct)).ToResponse();

    public async Task<CustomerResponse> UpdateMineAsync(SaveCustomerRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var entity = await GetMineOrThrowAsync(ct);

      
        var loginEmail = entity.Email;
        request.ApplyTo(entity);
        entity.Email = loginEmail;

        _customers.Update(entity);
        await _uow.SaveChangesAsync(ct);

        return entity.ToResponse();
    }

    private async Task<Customer> GetOrThrowAsync(int id, CancellationToken ct)
        => await _customers.GetByIdAsync(id, ct)
           ?? throw new NotFoundException(nameof(Customer), id);
  
    private async Task<Customer> GetMineOrThrowAsync(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("You must be logged in.");

        return await _customers.FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException("Customer profile", userId);
    }

    private static Func<IQueryable<Customer>, IOrderedQueryable<Customer>> GetOrderBy(CustomerQuery q)
    {
        var desc = q.Descending;
        return (q.SortBy?.ToLowerInvariant()) switch
        {
            "email" => src => desc ? src.OrderByDescending(x => x.Email) : src.OrderBy(x => x.Email),
            "created" => src => desc ? src.OrderByDescending(x => x.CreatedAt) : src.OrderBy(x => x.CreatedAt),
           
            _ => src => desc
                ? src.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName)
                : src.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
        };
    }
}