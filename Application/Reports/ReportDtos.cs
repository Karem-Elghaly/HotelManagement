
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reports;


public class ReportRangeQuery
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}

public record DailyOccupancy(DateOnly Date, int OccupiedRooms, decimal OccupancyRate);

public record OccupancyReport(
    DateOnly From,
    DateOnly To,
    int Days,
    int TotalRooms,             
    int AvailableRoomNights,    
    int OccupiedRoomNights,     
    decimal OccupancyRate,      
    decimal RoomRevenue,        
    decimal Adr,              
    decimal RevPar,            
    IReadOnlyList<DailyOccupancy> Daily);

public record DailyRevenue(DateOnly Date, decimal Payments, decimal Refunds, decimal Net);
public record MethodRevenue(PaymentMethod Method, decimal Net);
public record ExtraRevenue(string ServiceName, int Quantity, decimal Revenue);

public record RevenueReport(
    DateOnly From,
    DateOnly To,
    decimal TotalPayments,
    decimal TotalRefunds,
    decimal NetRevenue,                        
    IReadOnlyList<DailyRevenue> Daily,          
    IReadOnlyList<MethodRevenue> ByMethod,
    IReadOnlyList<ExtraRevenue> TopExtras);     


public record DashboardResponse(
    DateOnly Date,
    IReadOnlyDictionary<string, int> RoomsByStatus, 
    int ArrivalsToday,         
    int DeparturesToday,        
    int InHouse,                
    int PendingReservations,
    int OpenHousekeepingTasks,
    int OpenMaintenanceRequests);