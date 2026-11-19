
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.ExtraServices;

public record SaveExtraServiceRequest(string Name, string? Description, decimal Price, bool IsActive);

public record ExtraServiceResponse(int Id, string Name, string? Description, decimal Price, bool IsActive);

public static class ExtraServiceMappings
{
    public static ExtraServiceResponse ToResponse(this ExtraService e)
        => new(e.Id, e.Name, e.Description, e.Price, e.IsActive);

    public static ExtraService ToEntity(this SaveExtraServiceRequest r) => new()
    {
        Name = r.Name.Trim(),
        Description = r.Description?.Trim(),
        Price = r.Price,
        IsActive = r.IsActive
    };

    public static void ApplyTo(this SaveExtraServiceRequest r, ExtraService e)
    {
        e.Name = r.Name.Trim();
        e.Description = r.Description?.Trim();
        e.Price = r.Price;
        e.IsActive = r.IsActive;
    }
}