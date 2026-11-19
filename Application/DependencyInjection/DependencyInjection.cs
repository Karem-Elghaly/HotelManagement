
using FluentValidation;
using HotelManagement.Application.Availability;
using HotelManagement.Application.Billing;
using HotelManagement.Application.Customers;
using HotelManagement.Application.ExtraServices;
using HotelManagement.Application.Housekeeping;
using HotelManagement.Application.Maintenance;
using HotelManagement.Application.Pricing;
using HotelManagement.Application.Reports;
using HotelManagement.Application.Reservations;
using HotelManagement.Application.Reviews;
using HotelManagement.Application.RoomTypes;
using HotelManagement.Application.Rooms;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
   
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<ISeasonalRateService, SeasonalRateService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IExtraCatalogService, ExtraCatalogService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IHousekeepingService, HousekeepingService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}