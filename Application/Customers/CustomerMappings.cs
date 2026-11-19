
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Customers;

public static class CustomerMappings
{
    public static CustomerResponse ToResponse(this Customer e) => new(
        e.Id, $"{e.FirstName} {e.LastName}", e.FirstName, e.LastName,
        e.Email, e.Phone, e.NationalId, e.Address,
        HasAccount: e.UserId is not null);

    public static Customer ToEntity(this SaveCustomerRequest r) => new()
    {
        FirstName = r.FirstName.Trim(),
        LastName = r.LastName.Trim(),
        Email = r.Email.Trim(),
        Phone = r.Phone?.Trim(),
        NationalId = r.NationalId?.Trim(),
        Address = r.Address?.Trim()
    };

    public static void ApplyTo(this SaveCustomerRequest r, Customer e)
    {
        e.FirstName = r.FirstName.Trim();
        e.LastName = r.LastName.Trim();
        e.Email = r.Email.Trim();
        e.Phone = r.Phone?.Trim();
        e.NationalId = r.NationalId?.Trim();
        e.Address = r.Address?.Trim();
    }
}