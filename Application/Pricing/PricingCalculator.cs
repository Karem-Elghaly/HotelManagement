
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Pricing;


internal static class PricingCalculator
{
   
    public static bool IsWeekend(DateOnly d)
        => d.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;

    public static PriceQuote Calculate(
        RoomType roomType, DateOnly checkIn, DateOnly checkOut, IReadOnlyList<SeasonalRate> rules)
    {
        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var breakdown = new List<NightPrice>(nights);

       
        for (var day = checkIn; day < checkOut; day = day.AddDays(1))
        {
            var d = day;   

            var rule = rules
                .Where(r => r.IsActive
                            && (r.RoomTypeId == null || r.RoomTypeId == roomType.Id)  
                            && r.StartDate <= d && d < r.EndDate                       
                            && (!r.WeekendsOnly || IsWeekend(d)))                     
                .OrderByDescending(r => r.Priority)                
                .ThenByDescending(r => r.RoomTypeId.HasValue)       
                .ThenBy(r => r.Id)                                 
                .FirstOrDefault();

            var price = rule is null
                ? roomType.BasePricePerNight
                : Math.Round(roomType.BasePricePerNight * rule.Multiplier, 2, MidpointRounding.AwayFromZero);

            breakdown.Add(new NightPrice(d, price, rule?.Name));
        }

        var total = breakdown.Sum(n => n.Price);
        var average = nights == 0 ? 0m : Math.Round(total / nights, 2);

        return new PriceQuote(nights, total, average, breakdown);
    }
}