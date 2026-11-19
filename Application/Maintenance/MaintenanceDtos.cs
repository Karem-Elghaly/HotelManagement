
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Maintenance;

public record CreateMaintenanceRequest(
    int RoomId,
    string Title,
    string? Description,
    MaintenancePriority Priority,
    bool BlocksRoom,
    DateOnly? BlockedFrom,     
    DateOnly? BlockedUntil);

public record ResolveMaintenanceRequest(string? ResolutionNotes);

public record MaintenanceResponse(
    int Id,
    int RoomId,
    string RoomNumber,
    string Title,
    string? Description,
    MaintenancePriority Priority,
    MaintenanceStatus Status,
    bool BlocksRoom,
    DateOnly? BlockedFrom,
    DateOnly? BlockedUntil,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    string? ResolutionNotes);


public class MaintenanceQuery
{
    public string? Search { get; set; }
    public MaintenanceStatus? Status { get; set; }
    public bool Open { get; set; }               
    public MaintenancePriority? Priority { get; set; }
    public int? RoomId { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public static class MaintenanceMappings
{
 
    public static MaintenanceResponse ToResponse(this MaintenanceRequest m) => new(
        m.Id, m.RoomId, m.Room.Number, m.Title, m.Description, m.Priority, m.Status,
        m.BlocksRoom, m.BlockedFrom, m.BlockedUntil, m.CreatedAt, m.ResolvedAt, m.ResolutionNotes);
}