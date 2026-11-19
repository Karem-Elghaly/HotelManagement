
using System.Linq.Expressions;
using FluentValidation;
using HotelManagement.Application.Availability;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Reservations;

public class ReservationService : IReservationService
{

    private static readonly Expression<Func<Reservation, object>>[] Includes =
        [r => r.Room.RoomType, r => r.Customer];

    private readonly IRepository<Reservation> _reservations;
    private readonly IRepository<Room> _rooms;
    private readonly IRepository<RoomType> _roomTypes;
    private readonly IRepository<Customer> _customers;
    private readonly IRepository<Hotel> _hotels;
    private readonly IRoomAvailabilityQuery _availability;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateReservationRequest> _createValidator;

    public ReservationService(
        IRepository<Reservation> reservations,
        IRepository<Room> rooms,
        IRepository<RoomType> roomTypes,
        IRepository<Customer> customers,
        IRepository<Hotel> hotels,
        IRoomAvailabilityQuery availability,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IValidator<CreateReservationRequest> createValidator)
    {
        _reservations = reservations;
        _rooms = rooms;
        _roomTypes = roomTypes;
        _customers = customers;
        _hotels = hotels;
        _availability = availability;
        _uow = uow;
        _currentUser = currentUser;
        _createValidator = createValidator;
    }


    private bool IsFrontDesk =>
        _currentUser.IsInRole(Roles.Admin) ||
        _currentUser.IsInRole(Roles.Manager) ||
        _currentUser.IsInRole(Roles.Receptionist);



    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);

        var customer = await ResolveCustomerAsync(request, ct);

        var roomType = await _roomTypes.GetByIdAsync(request.RoomTypeId, ct)
            ?? throw new NotFoundException(nameof(RoomType), request.RoomTypeId);

        if (request.Adults > roomType.MaxAdults || request.Children > roomType.MaxChildren)
            throw new DomainException(
                $"'{roomType.Name}' fits up to {roomType.MaxAdults} adults and {roomType.MaxChildren} children.");

        
        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
        var totalPrice = roomType.BasePricePerNight * nights;

       
        var status = IsFrontDesk ? ReservationStatus.Confirmed : ReservationStatus.Pending;

        
        var newId = await _uow.ExecuteInTransactionAsync(async () =>
        {
            var roomId = await PickRoomIdAsync(request, ct);

            var reservation = new Reservation
            {
                ConfirmationCode = await GenerateUniqueCodeAsync(ct),
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                Adults = request.Adults,
                Children = request.Children,
                Status = status,
                TotalPrice = totalPrice,
                SpecialRequests = request.SpecialRequests?.Trim(),
                RoomId = roomId,          
                CustomerId = customer.Id
            };

            await _reservations.AddAsync(reservation, ct);
            await _uow.SaveChangesAsync(ct);
            return reservation.Id;
        }, ct);

        
        return (await GetOrThrowAsync(newId, ct)).ToResponse();
    }

   
    private async Task<Customer> ResolveCustomerAsync(CreateReservationRequest request, CancellationToken ct)
    {
        if (IsFrontDesk)
        {
            if (request.CustomerId is null)
                throw new DomainException("CustomerId is required when booking on behalf of a guest.");

            return await _customers.GetByIdAsync(request.CustomerId.Value, ct)
                ?? throw new NotFoundException(nameof(Customer), request.CustomerId.Value);
        }

       
        if (request.CustomerId is not null || request.RoomId is not null)
            throw new DomainException("Customers cannot specify CustomerId or RoomId.");

        var userId = _currentUser.UserId ?? throw new UnauthorizedException("You must be logged in.");

        return await _customers.FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException("Customer profile", userId);
    }


    private async Task<int> PickRoomIdAsync(CreateReservationRequest r, CancellationToken ct)
    {
  
        if (r.RoomId is not null && IsFrontDesk)
        {
            var room = await _rooms.GetByIdAsync(r.RoomId.Value, ct)
                ?? throw new NotFoundException(nameof(Room), r.RoomId.Value);

            if (room.RoomTypeId != r.RoomTypeId)
                throw new DomainException("The selected room does not belong to the selected room type.");

            if (room.Status == RoomStatus.OutOfService)
                throw new DomainException($"Room {room.Number} is out of service.");

            await _availability.LockRoomAsync(room.Id, ct);

            if (!await _availability.IsRoomAvailableAsync(room.Id, r.CheckInDate, r.CheckOutDate, null, ct))
                throw new DomainException($"Room {room.Number} is not available for the selected dates.");

            return room.Id;
        }

        
        var candidates = await _availability.GetAvailableRoomsAsync(
            r.CheckInDate, r.CheckOutDate, r.Adults, r.Children, r.RoomTypeId, ct);

      
        foreach (var candidate in candidates.Take(5))
        {
            await _availability.LockRoomAsync(candidate.Id, ct);

            if (await _availability.IsRoomAvailableAsync(candidate.Id, r.CheckInDate, r.CheckOutDate, null, ct))
                return candidate.Id;
        }

        throw new DomainException("No rooms of this type are available for the selected dates.");
    }


    private async Task<string> GenerateUniqueCodeAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = ConfirmationCodeGenerator.Generate();
            if (!await _reservations.ExistsAsync(x => x.ConfirmationCode == code, ct))
                return code;
        }

        throw new DomainException("Could not generate a confirmation code. Please try again.");
    }


    public Task<PagedResult<ReservationResponse>> GetAllAsync(ReservationQuery query, CancellationToken ct = default)
        => SearchAsync(query, forcedCustomerId: null, ct);

    public async Task<PagedResult<ReservationResponse>> GetMineAsync(ReservationQuery query, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("You must be logged in.");

        var me = await _customers.FirstOrDefaultAsync(c => c.UserId == userId, ct)
            ?? throw new NotFoundException("Customer profile", userId);

       
        return await SearchAsync(query, forcedCustomerId: me.Id, ct);
    }

    private async Task<PagedResult<ReservationResponse>> SearchAsync(
        ReservationQuery query, int? forcedCustomerId, CancellationToken ct)
    {
        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

    
        var term = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var status = query.Status;
        var from = query.CheckInFrom;
        var to = query.CheckInTo;
        var roomId = query.RoomId;
        var customerId = forcedCustomerId ?? query.CustomerId;

        Expression<Func<Reservation, bool>> filter = r =>
            (term == null ||
                r.ConfirmationCode.Contains(term) ||
                r.Room.Number.Contains(term) ||
                r.Customer.FirstName.Contains(term) ||
                r.Customer.LastName.Contains(term) ||
                r.Customer.Email.Contains(term)) &&
            (status == null || r.Status == status) &&
            (from == null || r.CheckInDate >= from) &&
            (to == null || r.CheckInDate <= to) &&
            (roomId == null || r.RoomId == roomId) &&
            (customerId == null || r.CustomerId == customerId);

        var (items, total) = await _reservations.GetPagedAsync(
            filter, GetOrderBy(query), pageNumber, pageSize, ct, Includes);

        return new PagedResult<ReservationResponse>(
            items.Select(e => e.ToResponse()).ToList(), total, pageNumber, pageSize);
    }

    public async Task<ReservationResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await GetAccessibleAsync(id, ct)).ToResponse();

    public async Task<ReservationResponse> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalized = code.Trim().ToUpperInvariant();

        var entity = await _reservations.FirstOrDefaultAsync(r => r.ConfirmationCode == normalized, ct, Includes)
            ?? throw new NotFoundException(nameof(Reservation), code);

        return entity.ToResponse();
    }

    public async Task<ReservationResponse> ConfirmAsync(int id, CancellationToken ct = default)
    {
        var r = await GetOrThrowAsync(id, ct);
        EnsureStatus(r, "confirm", ReservationStatus.Pending);

        r.Status = ReservationStatus.Confirmed;
        await _uow.SaveChangesAsync(ct);
        return r.ToResponse();
    }

   
    public async Task<ReservationResponse> CancelAsync(int id, CancellationToken ct = default)
    {
        var r = await GetAccessibleAsync(id, ct);  
        EnsureStatus(r, "cancel", ReservationStatus.Pending, ReservationStatus.Confirmed);

       
        if (!IsFrontDesk)
        {
            var hotel = await _hotels.FirstOrDefaultAsync(_ => true, ct)
                ?? throw new NotFoundException(nameof(Hotel), "default");

       
            var checkInAt = r.CheckInDate.ToDateTime(hotel.CheckInTime);
            var deadline = checkInAt.AddHours(-hotel.FreeCancellationHours);

            if (DateTime.UtcNow > deadline)
                throw new DomainException(
                    $"Free cancellation ended {hotel.FreeCancellationHours} hours before check-in. Please contact the hotel.");
        }

        r.Status = ReservationStatus.Cancelled;
        r.CancelledAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);
        return r.ToResponse();
    }

  
    public async Task<ReservationResponse> CheckInAsync(int id, CancellationToken ct = default)
    {
        var r = await GetOrThrowAsync(id, ct);
        EnsureStatus(r, "check in", ReservationStatus.Confirmed);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today < r.CheckInDate)
            throw new DomainException("Check-in is not allowed before the reservation start date.");
        if (today >= r.CheckOutDate)
            throw new DomainException("The reservation period has already ended.");

        if (r.Room.Status != RoomStatus.Available)
            throw new DomainException($"Room {r.Room.Number} is not ready (status: {r.Room.Status}).");

        r.Status = ReservationStatus.CheckedIn;
        r.Room.Status = RoomStatus.Occupied;   
        await _uow.SaveChangesAsync(ct);
        return r.ToResponse();
    }

    
    public async Task<ReservationResponse> CheckOutAsync(int id, CancellationToken ct = default)
    {
        var r = await GetOrThrowAsync(id, ct);
        EnsureStatus(r, "check out", ReservationStatus.CheckedIn);

        r.Status = ReservationStatus.CheckedOut;
        r.Room.Status = RoomStatus.Cleaning;   
        await _uow.SaveChangesAsync(ct);

        return r.ToResponse();
    }

    public async Task<ReservationResponse> MarkNoShowAsync(int id, CancellationToken ct = default)
    {
        var r = await GetOrThrowAsync(id, ct);
        EnsureStatus(r, "mark as no-show", ReservationStatus.Confirmed);

        if (DateOnly.FromDateTime(DateTime.UtcNow) < r.CheckInDate)
            throw new DomainException("A reservation cannot be a no-show before its check-in date.");

        r.Status = ReservationStatus.NoShow;
        await _uow.SaveChangesAsync(ct);
        return r.ToResponse();
    }


    private static void EnsureStatus(Reservation r, string action, params ReservationStatus[] allowed)
    {
        if (!allowed.Contains(r.Status))
            throw new DomainException($"Cannot {action} a reservation that is {r.Status}.");
    }

   
    private async Task<Reservation> GetOrThrowAsync(int id, CancellationToken ct)
        => await _reservations.GetByIdAsync(id, ct, Includes)
           ?? throw new NotFoundException(nameof(Reservation), id);

   
    private async Task<Reservation> GetAccessibleAsync(int id, CancellationToken ct)
    {
        var r = await GetOrThrowAsync(id, ct);

        if (!IsFrontDesk && r.Customer.UserId != _currentUser.UserId)
            throw new NotFoundException(nameof(Reservation), id);

        return r;
    }

    private static Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> GetOrderBy(ReservationQuery q)
    {
        var desc = q.Descending;
     
        return (q.SortBy?.ToLowerInvariant()) switch
        {
            "price" => src => desc ? src.OrderByDescending(x => x.TotalPrice).ThenBy(x => x.Id)
                                     : src.OrderBy(x => x.TotalPrice).ThenBy(x => x.Id),
            "created" => src => desc ? src.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
                                     : src.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            "status" => src => desc ? src.OrderByDescending(x => x.Status).ThenBy(x => x.Id)
                                     : src.OrderBy(x => x.Status).ThenBy(x => x.Id),
            _ => src => desc ? src.OrderByDescending(x => x.CheckInDate).ThenBy(x => x.Id)
                                     : src.OrderBy(x => x.CheckInDate).ThenBy(x => x.Id)
        };
    }
}