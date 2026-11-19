
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Pricing;

public interface IPricingService
{
    
    Task<PriceQuote> GetQuoteAsync(QuoteQuery query, CancellationToken ct = default);

    
    Task<PriceQuote> CalculateAsync(RoomType roomType, DateOnly checkIn, DateOnly checkOut, CancellationToken ct = default);
}

public interface ISeasonalRateService
{
    Task<IReadOnlyList<SeasonalRateResponse>> GetAllAsync(CancellationToken ct = default);
    Task<SeasonalRateResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SeasonalRateResponse> CreateAsync(SaveSeasonalRateRequest request, CancellationToken ct = default);
    Task<SeasonalRateResponse> UpdateAsync(int id, SaveSeasonalRateRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}