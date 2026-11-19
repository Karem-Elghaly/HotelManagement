
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Reviews;

public interface IReviewService
{
   
    Task<ReviewResponse> CreateAsync(int reservationId, SaveReviewRequest request, CancellationToken ct = default);
    Task<ReviewResponse> UpdateAsync(int reviewId, SaveReviewRequest request, CancellationToken ct = default);

  
    Task<PagedResult<ReviewResponse>> GetPublicAsync(ReviewQuery query, CancellationToken ct = default);
    Task<ReviewSummaryResponse> GetSummaryAsync(CancellationToken ct = default);


    Task<PagedResult<StaffReviewResponse>> GetForStaffAsync(ReviewQuery query, CancellationToken ct = default);
    Task<StaffReviewResponse> ReplyAsync(int reviewId, ReplyToReviewRequest request, CancellationToken ct = default);
    Task<StaffReviewResponse> SetHiddenAsync(int reviewId, bool hidden, CancellationToken ct = default);
}