
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.BackgroundJobs;


public class PendingReservationExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<PendingReservationExpiryService> _logger;


    public PendingReservationExpiryService(
        IServiceScopeFactory scopeFactory, IConfiguration config,
        ILogger<PendingReservationExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       
        var expiryHours = _config.GetValue("Reservations:PendingExpiryHours", 24);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

        do
        {
            try
            {
                await ExpireAsync(expiryHours, stoppingToken);
            }
           
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to expire pending reservations");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ExpireAsync(int expiryHours, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var cutoff = now.AddHours(-expiryHours);

       
        var count = await db.Reservations
            .Where(r => r.Status == ReservationStatus.Pending
                        && r.CreatedAt < cutoff
                        && !db.Payments.Any(p => p.ReservationId == r.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ReservationStatus.Cancelled)
                .SetProperty(r => r.CancelledAt, (DateTime?)now)
                .SetProperty(r => r.UpdatedAt, (DateTime?)now), ct);

        if (count > 0)
            _logger.LogInformation("Expired {Count} pending reservation(s)", count);
    }
}