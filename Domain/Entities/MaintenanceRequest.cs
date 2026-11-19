
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

public class MaintenanceRequest : BaseEntity
{
    
    public static readonly MaintenanceStatus[] ActiveStatuses =
        [MaintenanceStatus.Open, MaintenanceStatus.InProgress];

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Open;


    public bool BlocksRoom { get; set; }

  
    public DateOnly? BlockedFrom { get; set; }
    public DateOnly? BlockedUntil { get; set; }

    public string? ReportedByUserId { get; set; }

    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}