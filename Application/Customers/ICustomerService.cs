
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Customers;

public interface ICustomerService
{

    Task<PagedResult<CustomerResponse>> GetAllAsync(CustomerQuery query, CancellationToken ct = default);
    Task<CustomerResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CustomerResponse> CreateAsync(SaveCustomerRequest request, CancellationToken ct = default);
    Task<CustomerResponse> UpdateAsync(int id, SaveCustomerRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

 
    Task<CustomerResponse> GetMineAsync(CancellationToken ct = default);
    Task<CustomerResponse> UpdateMineAsync(SaveCustomerRequest request, CancellationToken ct = default);
}