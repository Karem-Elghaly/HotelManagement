
namespace HotelManagement.Application.ExtraServices;

public interface IExtraCatalogService
{
   
    Task<IReadOnlyList<ExtraServiceResponse>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    Task<ExtraServiceResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ExtraServiceResponse> CreateAsync(SaveExtraServiceRequest request, CancellationToken ct = default);
    Task<ExtraServiceResponse> UpdateAsync(int id, SaveExtraServiceRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}