
namespace HotelManagement.Application.Reports;


internal static class OccupancyCalculator
{
    public static OccupancyReport Calculate(
        IReadOnlyList<StayRow> stays, int totalRooms, DateOnly from, DateOnly to)
    {
        var days = to.DayNumber - from.DayNumber;


        var occupied = new int[days];
        decimal roomRevenue = 0;

        foreach (var stay in stays)
        {
         
            var start = stay.CheckIn > from ? stay.CheckIn : from;
            var end = stay.CheckOut < to ? stay.CheckOut : to;

            var nightsInRange = end.DayNumber - start.DayNumber;
            if (nightsInRange <= 0) continue;

           
            for (var d = start.DayNumber; d < end.DayNumber; d++)
                occupied[d - from.DayNumber]++;

        
            var totalNights = stay.CheckOut.DayNumber - stay.CheckIn.DayNumber;
            roomRevenue += stay.TotalPrice / totalNights * nightsInRange;
        }

        var occupiedNights = occupied.Sum();
        var availableNights = totalRooms * days;

  
        decimal Pct(int part, int whole) => whole == 0 ? 0m : Math.Round(part * 100m / whole, 1);

        var daily = Enumerable.Range(0, days)
            .Select(i => new DailyOccupancy(
                DateOnly.FromDayNumber(from.DayNumber + i), occupied[i], Pct(occupied[i], totalRooms)))
            .ToList();

        roomRevenue = Math.Round(roomRevenue, 2);

        return new OccupancyReport(
            from, to, days, totalRooms, availableNights, occupiedNights,
            Pct(occupiedNights, availableNights),
            roomRevenue,
            Adr: occupiedNights == 0 ? 0m : Math.Round(roomRevenue / occupiedNights, 2),
            RevPar: availableNights == 0 ? 0m : Math.Round(roomRevenue / availableNights, 2),
            daily);
    }
}