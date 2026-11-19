// Pricing/PricingService.cs
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Pricing;

public class PricingService : IPricingService
{
    private readonly IRepository<SeasonalRate> _rates;
    private readonly IRepository<RoomType> _roomTypes;
    private readonly IValidator<QuoteQuery> _validator;

    public PricingService(
        IRepository<SeasonalRate> rates, IRepository<RoomType> roomTypes, IValidator<QuoteQuery> validator)
    {
        _rates = rates;
        _roomTypes = roomTypes;
        _validator = validator;
    }

    public async Task<PriceQuote> GetQuoteAsync(QuoteQuery query, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(query, ct);

        var type = await _roomTypes.GetByIdAsync(query.RoomTypeId, ct)
            ?? throw new NotFoundException(nameof(RoomType), query.RoomTypeId);

        return await CalculateAsync(type, query.CheckIn, query.CheckOut, ct);
    }

    public async Task<PriceQuote> CalculateAsync(
        RoomType roomType, DateOnly checkIn, DateOnly checkOut, CancellationToken ct = default)
    {
        var typeId = roomType.Id;   

        
        var rules = await _rates.ListAsync(r =>
            r.IsActive
            && (r.RoomTypeId == null || r.RoomTypeId == typeId)
            && r.StartDate < checkOut
            && r.EndDate > checkIn, ct);

        return PricingCalculator.Calculate(roomType, checkIn, checkOut, rules);
    }
}

public class SeasonalRateService : ISeasonalRateService
{
    private readonly IRepository<SeasonalRate> _rates;
    private readonly IRepository<RoomType> _roomTypes;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SaveSeasonalRateRequest> _validator;

    public SeasonalRateService(
        IRepository<SeasonalRate> rates, IRepository<RoomType> roomTypes,
        IUnitOfWork uow, IValidator<SaveSeasonalRateRequest> validator)
    {
        _rates = rates;
        _roomTypes = roomTypes;
        _uow = uow;
        _validator = validator;
    }

  
    public async Task<IReadOnlyList<SeasonalRateResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _rates.ListAsync(_ => true, ct);
        return list.OrderBy(r => r.StartDate).ThenBy(r => r.Id).Select(r => r.ToResponse()).ToList();
    }

    public async Task<SeasonalRateResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await GetOrThrowAsync(id, ct)).ToResponse();

    public async Task<SeasonalRateResponse> CreateAsync(SaveSeasonalRateRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        await EnsureRoomTypeExistsAsync(request.RoomTypeId, ct);

        var entity = request.ToEntity();
        await _rates.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return entity.ToResponse();
    }

    public async Task<SeasonalRateResponse> UpdateAsync(int id, SaveSeasonalRateRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        await EnsureRoomTypeExistsAsync(request.RoomTypeId, ct);

        var entity = await GetOrThrowAsync(id, ct);
        request.ApplyTo(entity);   

        await _uow.SaveChangesAsync(ct);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetOrThrowAsync(id, ct);
        _rates.Remove(entity);   
        await _uow.SaveChangesAsync(ct);
    }

    private async Task EnsureRoomTypeExistsAsync(int? roomTypeId, CancellationToken ct)
    {
        if (roomTypeId is not null && !await _roomTypes.ExistsAsync(t => t.Id == roomTypeId, ct))
            throw new NotFoundException(nameof(RoomType), roomTypeId.Value);
    }

    private async Task<SeasonalRate> GetOrThrowAsync(int id, CancellationToken ct)
        => await _rates.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(SeasonalRate), id);
}