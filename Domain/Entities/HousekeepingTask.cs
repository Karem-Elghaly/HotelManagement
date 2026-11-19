
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

public class HousekeepingTask : BaseEntity
{
   
    public static readonly HousekeepingTaskStatus[] OpenStatuses =
        [HousekeepingTaskStatus.Pending, HousekeepingTaskStatus.InProgress];

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public HousekeepingTaskType Type { get; set; }
    public HousekeepingTaskStatus Status { get; set; } = HousekeepingTaskStatus.Pending;

    public string? AssignedToUserId { get; set; }

    public string? Notes { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

  
    public string? CreatedByUserId { get; set; }

    
    public byte[] RowVersion { get; set; } = [];
}