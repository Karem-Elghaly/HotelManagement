
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Maintenance;

public interface IMaintenanceService
{
    Task<PagedResult<MaintenanceResponse>> GetAllAsync(MaintenanceQuery query, CancellationToken ct = default);
    Task<MaintenanceResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MaintenanceResponse> CreateAsync(CreateMaintenanceRequest request, CancellationToken ct = default);
    Task<MaintenanceResponse> StartAsync(int id, CancellationToken ct = default);
    Task<MaintenanceResponse> ResolveAsync(int id, ResolveMaintenanceRequest request, CancellationToken ct = default);
    Task<MaintenanceResponse> CancelAsync(int id, CancellationToken ct = default);
}