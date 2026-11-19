
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities;

public class Customer : BaseEntity
{

    public string? UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    
    public string? NationalId { get; set; }
    public string? Address { get; set; }
}