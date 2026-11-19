
namespace HotelManagement.Application.Customers;


public record SaveCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? NationalId,
    string? Address);

public record CustomerResponse(
    int Id,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? NationalId,
    string? Address,
    bool HasAccount);   


public class CustomerQuery
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}