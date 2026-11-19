
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Housekeeping;


public record CreateHousekeepingTaskRequest(int RoomId, HousekeepingTaskType Type, string? Notes);


public record CompleteHousekeepingTaskRequest(string? Notes);

public record HousekeepingTaskResponse(
    int Id,
    int RoomId,
    string RoomNumber,
    int Floor,
    HousekeepingTaskType Type,
    HousekeepingTaskStatus Status,
    string? AssignedToUserId,
    string? Notes,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);


public class HousekeepingTaskQuery
{
    public HousekeepingTaskStatus? Status { get; set; }
    public bool Open { get; set; }         
    public bool Mine { get; set; }          
    public bool Unassigned { get; set; }   
    public int? RoomId { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public static class HousekeepingMappings
{

    public static HousekeepingTaskResponse ToResponse(this HousekeepingTask t) => new(
        t.Id, t.RoomId, t.Room.Number, t.Room.Floor, t.Type, t.Status,
        t.AssignedToUserId, t.Notes, t.CreatedAt, t.StartedAt, t.CompletedAt);
}