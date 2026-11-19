
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Housekeeping;

public interface IHousekeepingService
{
    Task<PagedResult<HousekeepingTaskResponse>> GetAllAsync(HousekeepingTaskQuery query, CancellationToken ct = default);
    Task<HousekeepingTaskResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<HousekeepingTaskResponse> CreateAsync(CreateHousekeepingTaskRequest request, CancellationToken ct = default);
    Task<HousekeepingTaskResponse> StartAsync(int id, CancellationToken ct = default);
    Task<HousekeepingTaskResponse> CompleteAsync(int id, CompleteHousekeepingTaskRequest request, CancellationToken ct = default);
    Task<HousekeepingTaskResponse> CancelAsync(int id, CancellationToken ct = default);
}