using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence;


public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        
        if (await db.RoomTypes.IgnoreQueryFilters().AnyAsync())
            return;

        var single = new RoomType
        {
            Name = "Single",
            Description = "Cozy room for one guest",
            BasePricePerNight = 800m,
            MaxAdults = 1,
            MaxChildren = 0,
            AreaInSquareMeters = 20
        };
        var dbl = new RoomType
        {
            Name = "Double",
            Description = "Comfortable room for two guests",
            BasePricePerNight = 1200m,
            MaxAdults = 2,
            MaxChildren = 1,
            AreaInSquareMeters = 30
        };
        var suite = new RoomType
        {
            Name = "Suite",
            Description = "Spacious suite with a living area",
            BasePricePerNight = 2500m,
            MaxAdults = 4,
            MaxChildren = 2,
            AreaInSquareMeters = 60
        };

     
        db.Rooms.AddRange(
            new Room { Number = "101", Floor = 1, RoomType = single },
            new Room { Number = "102", Floor = 1, RoomType = single },
            new Room { Number = "103", Floor = 1, RoomType = single },
            new Room { Number = "201", Floor = 2, RoomType = dbl },
            new Room { Number = "202", Floor = 2, RoomType = dbl },
            new Room { Number = "203", Floor = 2, RoomType = dbl },
            new Room { Number = "204", Floor = 2, RoomType = dbl },
            new Room { Number = "301", Floor = 3, RoomType = suite },
            new Room { Number = "302", Floor = 3, RoomType = suite },
            new Room { Number = "303", Floor = 3, RoomType = suite });

     
        db.SeasonalRates.Add(new SeasonalRate
        {
            Name = "Weekend",
            RoomTypeId = null,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2030, 1, 1),
            Multiplier = 1.25m,
            WeekendsOnly = true,
            Priority = 5,
            IsActive = true
        });

        await db.SaveChangesAsync();
    }
}