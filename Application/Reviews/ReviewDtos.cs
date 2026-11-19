
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Reviews;


public record SaveReviewRequest(int Rating, string? Comment);

public record ReplyToReviewRequest(string Reply);


public record ReviewResponse(
    int Id,
    int Rating,
    string? Comment,
    string ReviewerName,          
    DateTime CreatedAt,
    string? ManagerReply,
    DateTime? RepliedAt);

public record StaffReviewResponse(
    int Id,
    int ReservationId,
    int CustomerId,
    string CustomerName,
    int Rating,
    string? Comment,
    bool IsHidden,
    DateTime CreatedAt,
    string? ManagerReply,
    DateTime? RepliedAt);


public record ReviewSummaryResponse(
    double AverageRating,
    int TotalReviews,
    IReadOnlyDictionary<int, int> Distribution);   

public class ReviewQuery
{
    public int? Rating { get; set; }        
    public int? MinRating { get; set; }    
    public bool? IsHidden { get; set; }     
    public string? SortBy { get; set; }     
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public static class ReviewMappings
{
 
    public static ReviewResponse ToResponse(this Review r) => new(
        r.Id, r.Rating, r.Comment, ReviewerName(r.Customer), r.CreatedAt, r.ManagerReply, r.RepliedAt);

    public static StaffReviewResponse ToStaffResponse(this Review r) => new(
        r.Id, r.ReservationId, r.CustomerId, $"{r.Customer.FirstName} {r.Customer.LastName}",
        r.Rating, r.Comment, r.IsHidden, r.CreatedAt, r.ManagerReply, r.RepliedAt);


    private static string ReviewerName(Customer c)
    {
        var last = c.LastName.Trim();
        return last.Length == 0 ? c.FirstName.Trim() : $"{c.FirstName.Trim()} {last[0]}.";
    }
}