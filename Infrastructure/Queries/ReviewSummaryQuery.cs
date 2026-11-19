
using HotelManagement.Application.Reviews;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence.Queries;

public class ReviewSummaryQuery : IReviewSummaryQuery
{
    private readonly AppDbContext _db;

    public ReviewSummaryQuery(AppDbContext db) => _db = db;

    public async Task<ReviewSummaryResponse> GetAsync(CancellationToken ct = default)
    {
    
        var groups = await _db.Reviews
            .AsNoTracking()
            .Where(r => !r.IsHidden)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(ct);

     
        var distribution = Enumerable.Range(1, 5)
            .ToDictionary(star => star, star => groups.FirstOrDefault(g => g.Rating == star)?.Count ?? 0);

        var total = distribution.Values.Sum();
        var sum = distribution.Sum(kv => kv.Key * kv.Value);

        return new ReviewSummaryResponse(
            AverageRating: total == 0 ? 0 : Math.Round((double)sum / total, 2),
            TotalReviews: total,
            Distribution: distribution);
    }
}