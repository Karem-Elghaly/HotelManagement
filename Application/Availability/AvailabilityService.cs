
using FluentValidation;
using HotelManagement.Application.Pricing;

namespace HotelManagement.Application.Availability;

public class AvailabilityService : IAvailabilityService
{
    private readonly IRoomAvailabilityQuery _availability;
    private readonly IPricingService _pricing;
    private readonly IValidator<AvailabilityQuery> _validator;

    public AvailabilityService(
        IRoomAvailabilityQuery availability, IPricingService pricing, IValidator<AvailabilityQuery> validator)
    {
        _availability = availability;
        _pricing = pricing;
        _validator = validator;
    }

    public async Task<IReadOnlyList<RoomTypeAvailabilityResponse>> SearchAsync(
        AvailabilityQuery query, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(query, ct);

        var rooms = await _availability.GetAvailableRoomsAsync(
            query.CheckIn, query.CheckOut, query.Adults, query.Children, query.RoomTypeId, ct);

        var result = new List<RoomTypeAvailabilityResponse>();

      
        foreach (var group in rooms.GroupBy(r => r.RoomTypeId))
        {
            var type = group.First().RoomType;
            var quote = await _pricing.CalculateAsync(type, query.CheckIn, query.CheckOut, ct);

            result.Add(new RoomTypeAvailabilityResponse(
                type.Id, type.Name, type.Description,
                type.MaxAdults, type.MaxChildren, type.AreaInSquareMeters,
                quote.AveragePerNight,  
                quote.Nights, quote.TotalPrice, group.Count()));
        }

        return result.OrderBy(x => x.TotalPrice).ToList();   
    }

    public async Task<IReadOnlyList<AvailableRoomResponse>> GetAvailableRoomsAsync(
        AvailabilityQuery query, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(query, ct);

        var rooms = await _availability.GetAvailableRoomsAsync(
            query.CheckIn, query.CheckOut, query.Adults, query.Children, query.RoomTypeId, ct);

   
        var quotes = new Dictionary<int, PriceQuote>();
        var result = new List<AvailableRoomResponse>();

        foreach (var r in rooms)
        {
            if (!quotes.TryGetValue(r.RoomTypeId, out var quote))
            {
                quote = await _pricing.CalculateAsync(r.RoomType, query.CheckIn, query.CheckOut, ct);
                quotes[r.RoomTypeId] = quote;
            }

            result.Add(new AvailableRoomResponse(
                r.Id, r.Number, r.Floor, r.RoomTypeId, r.RoomType.Name,
                quote.AveragePerNight, quote.TotalPrice));
        }

        return result;
    }
}