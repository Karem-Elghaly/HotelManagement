
namespace HotelManagement.Application.Reviews;


public interface IReviewSummaryQuery
{
    Task<ReviewSummaryResponse> GetAsync(CancellationToken ct = default);
}