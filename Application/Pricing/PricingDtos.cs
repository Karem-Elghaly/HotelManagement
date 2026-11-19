
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Pricing;

public record SaveSeasonalRateRequest(
    string Name, int? RoomTypeId, DateOnly StartDate, DateOnly EndDate,
    decimal Multiplier, bool WeekendsOnly, int Priority, bool IsActive);

public record SeasonalRateResponse(
    int Id, string Name, int? RoomTypeId, DateOnly StartDate, DateOnly EndDate,
    decimal Multiplier, bool WeekendsOnly, int Priority, bool IsActive);


public record NightPrice(DateOnly Date, decimal Price, string? AppliedRule);

public record PriceQuote(
    int Nights, decimal TotalPrice, decimal AveragePerNight, IReadOnlyList<NightPrice> Breakdown);


public class QuoteQuery
{
    public int RoomTypeId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
}

public static class SeasonalRateMappings
{
    public static SeasonalRateResponse ToResponse(this SeasonalRate r) => new(
        r.Id, r.Name, r.RoomTypeId, r.StartDate, r.EndDate,
        r.Multiplier, r.WeekendsOnly, r.Priority, r.IsActive);

    public static SeasonalRate ToEntity(this SaveSeasonalRateRequest r)
    {
        var e = new SeasonalRate();
        r.ApplyTo(e);
        return e;
    }

    public static void ApplyTo(this SaveSeasonalRateRequest r, SeasonalRate e)
    {
        e.Name = r.Name.Trim();
        e.RoomTypeId = r.RoomTypeId;
        e.StartDate = r.StartDate;
        e.EndDate = r.EndDate;
        e.Multiplier = r.Multiplier;
        e.WeekendsOnly = r.WeekendsOnly;
        e.Priority = r.Priority;
        e.IsActive = r.IsActive;
    }
}