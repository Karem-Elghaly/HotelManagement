
using System.Linq.Expressions;
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Reviews;

public class ReviewService : IReviewService
{
   
    private const int EditWindowDays = 30;

   
    private static readonly Expression<Func<Review, object>>[] Includes = [r => r.Customer];

    private readonly IRepository<Review> _reviews;
    private readonly IRepository<Reservation> _reservations;
    private readonly IUnitOfWork _uow;
    private readonly IReservationLocker _locker;
    private readonly ICurrentUserService _currentUser;
    private readonly IReviewSummaryQuery _summary;
    private readonly IValidator<SaveReviewRequest> _saveValidator;
    private readonly IValidator<ReplyToReviewRequest> _replyValidator;

    public ReviewService(
        IRepository<Review> reviews,
        IRepository<Reservation> reservations,
        IUnitOfWork uow,
        IReservationLocker locker,
        ICurrentUserService currentUser,
        IReviewSummaryQuery summary,
        IValidator<SaveReviewRequest> saveValidator,
        IValidator<ReplyToReviewRequest> replyValidator)
    {
        _reviews = reviews;
        _reservations = reservations;
        _uow = uow;
        _locker = locker;
        _currentUser = currentUser;
        _summary = summary;
        _saveValidator = saveValidator;
        _replyValidator = replyValidator;
    }



    public async Task<ReviewResponse> CreateAsync(
        int reservationId, SaveReviewRequest request, CancellationToken ct = default)
    {
        await _saveValidator.ValidateAndThrowAsync(request, ct);

        var userId = _currentUser.UserId ?? throw new UnauthorizedException("You must be logged in.");

       
        var newId = await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _locker.LockAsync(reservationId, ct);

            var reservation = await _reservations.GetByIdAsync(reservationId, ct, r => r.Customer)
                ?? throw new NotFoundException(nameof(Reservation), reservationId);

           
            if (reservation.Customer.UserId != userId)
                throw new NotFoundException(nameof(Reservation), reservationId);

          
            if (reservation.Status != ReservationStatus.CheckedOut)
                throw new DomainException("You can review a stay only after check-out.");

            if (await _reviews.ExistsAsync(r => r.ReservationId == reservationId, ct))
                throw new DomainException("You have already reviewed this stay.");

            var review = new Review
            {
                ReservationId = reservation.Id,
                CustomerId = reservation.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim()
            };

            await _reviews.AddAsync(review, ct);
            await _uow.SaveChangesAsync(ct);
            return review.Id;
        }, ct);

        return (await GetOrThrowAsync(newId, ct)).ToResponse();
    }

    public async Task<ReviewResponse> UpdateAsync(
        int reviewId, SaveReviewRequest request, CancellationToken ct = default)
    {
        await _saveValidator.ValidateAndThrowAsync(request, ct);

        var userId = _currentUser.UserId ?? throw new UnauthorizedException("You must be logged in.");
        var review = await GetOrThrowAsync(reviewId, ct);

        if (review.Customer.UserId != userId)
            throw new NotFoundException(nameof(Review), reviewId);

        if (DateTime.UtcNow - review.CreatedAt > TimeSpan.FromDays(EditWindowDays))
            throw new DomainException($"Reviews can be edited only within {EditWindowDays} days.");

      
        review.Rating = request.Rating;
        review.Comment = request.Comment?.Trim();

        await _uow.SaveChangesAsync(ct);  
        return review.ToResponse();
    }

 

    public async Task<PagedResult<ReviewResponse>> GetPublicAsync(ReviewQuery query, CancellationToken ct = default)
    {
   
        var (items, total, page, size) = await SearchAsync(query, hidden: false, ct);
        return new PagedResult<ReviewResponse>(items.Select(r => r.ToResponse()).ToList(), total, page, size);
    }

    public Task<ReviewSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
        => _summary.GetAsync(ct);



    public async Task<PagedResult<StaffReviewResponse>> GetForStaffAsync(ReviewQuery query, CancellationToken ct = default)
    {
 
        var (items, total, page, size) = await SearchAsync(query, query.IsHidden, ct);
        return new PagedResult<StaffReviewResponse>(items.Select(r => r.ToStaffResponse()).ToList(), total, page, size);
    }

    public async Task<StaffReviewResponse> ReplyAsync(
        int reviewId, ReplyToReviewRequest request, CancellationToken ct = default)
    {
        await _replyValidator.ValidateAndThrowAsync(request, ct);

        var review = await GetOrThrowAsync(reviewId, ct);
        review.ManagerReply = request.Reply.Trim();
        review.RepliedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);
        return review.ToStaffResponse();
    }

    public async Task<StaffReviewResponse> SetHiddenAsync(int reviewId, bool hidden, CancellationToken ct = default)
    {
        var review = await GetOrThrowAsync(reviewId, ct);
        review.IsHidden = hidden;

        await _uow.SaveChangesAsync(ct);
        return review.ToStaffResponse();
    }

    
    private async Task<(IReadOnlyList<Review> Items, int Total, int Page, int Size)> SearchAsync(
        ReviewQuery query, bool? hidden, CancellationToken ct)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

     
        var rating = query.Rating;
        var minRating = query.MinRating;

        Expression<Func<Review, bool>> filter = r =>
            (hidden == null || r.IsHidden == hidden) &&
            (rating == null || r.Rating == rating) &&
            (minRating == null || r.Rating >= minRating);

        var (items, total) = await _reviews.GetPagedAsync(
            filter, GetOrderBy(query), pageNumber, pageSize, ct, Includes);

        return (items, total, pageNumber, pageSize);
    }

    private async Task<Review> GetOrThrowAsync(int id, CancellationToken ct)
        => await _reviews.GetByIdAsync(id, ct, Includes)
           ?? throw new NotFoundException(nameof(Review), id);

    private static Func<IQueryable<Review>, IOrderedQueryable<Review>> GetOrderBy(ReviewQuery q)
    {
        
        return (q.SortBy?.ToLowerInvariant()) switch
        {
            "highest" => src => src.OrderByDescending(x => x.Rating).ThenByDescending(x => x.CreatedAt).ThenBy(x => x.Id),
            "lowest" => src => src.OrderBy(x => x.Rating).ThenByDescending(x => x.CreatedAt).ThenBy(x => x.Id),
            _ => src => src.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
        };
    }
}