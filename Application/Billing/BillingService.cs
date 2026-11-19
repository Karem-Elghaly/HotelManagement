
using FluentValidation;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Exceptions;

namespace HotelManagement.Application.Billing;

public class BillingService : IBillingService
{
    
    private const decimal DepositRate = 0.30m;


    private static readonly ReservationStatus[] ExtrasAllowed =
        [ReservationStatus.Pending, ReservationStatus.Confirmed, ReservationStatus.CheckedIn];

    
    private static readonly ReservationStatus[] PaymentsAllowed =
        [ReservationStatus.Pending, ReservationStatus.Confirmed, ReservationStatus.CheckedIn, ReservationStatus.CheckedOut];

    private readonly IRepository<Reservation> _reservations;
    private readonly IRepository<ReservationExtra> _extras;
    private readonly IRepository<ExtraService> _catalog;
    private readonly IRepository<Payment> _payments;
    private readonly IUnitOfWork _uow;
    private readonly IReservationLocker _locker;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<AddExtraRequest> _extraValidator;
    private readonly IValidator<CreatePaymentRequest> _paymentValidator;
    private readonly IValidator<RefundRequest> _refundValidator;

    public BillingService(
        IRepository<Reservation> reservations,
        IRepository<ReservationExtra> extras,
        IRepository<ExtraService> catalog,
        IRepository<Payment> payments,
        IUnitOfWork uow,
        IReservationLocker locker,
        ICurrentUserService currentUser,
        IValidator<AddExtraRequest> extraValidator,
        IValidator<CreatePaymentRequest> paymentValidator,
        IValidator<RefundRequest> refundValidator)
    {
        _reservations = reservations;
        _extras = extras;
        _catalog = catalog;
        _payments = payments;
        _uow = uow;
        _locker = locker;
        _currentUser = currentUser;
        _extraValidator = extraValidator;
        _paymentValidator = paymentValidator;
        _refundValidator = refundValidator;
    }

    private bool IsFrontDesk =>
        _currentUser.IsInRole(Roles.Admin) ||
        _currentUser.IsInRole(Roles.Manager) ||
        _currentUser.IsInRole(Roles.Receptionist);


    public async Task<BillResponse> GetBillAsync(int reservationId, CancellationToken ct = default)
    {
        var r = await GetAccessibleAsync(reservationId, ct);
        return await BuildBillAsync(r, ct);
    }

    public async Task<decimal> GetBalanceAsync(int reservationId, CancellationToken ct = default)
    {
        var r = await _reservations.GetByIdAsync(reservationId, ct)
            ?? throw new NotFoundException(nameof(Reservation), reservationId);

        return (await BuildBillAsync(r, ct)).Balance;
    }


    public async Task<BillResponse> AddExtraAsync(int reservationId, AddExtraRequest request, CancellationToken ct = default)
    {
        await _extraValidator.ValidateAndThrowAsync(request, ct);

       
        return await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _locker.LockAsync(reservationId, ct);

         
            var r = await GetAccessibleAsync(reservationId, ct);

            if (!ExtrasAllowed.Contains(r.Status))
                throw new DomainException($"Cannot add extras to a reservation that is {r.Status}.");

            var service = await _catalog.GetByIdAsync(request.ExtraServiceId, ct)
                ?? throw new NotFoundException(nameof(ExtraService), request.ExtraServiceId);

            if (!service.IsActive)
                throw new DomainException($"'{service.Name}' is not available right now.");

          
            await _extras.AddAsync(new ReservationExtra
            {
                ReservationId = r.Id,
                ExtraServiceId = service.Id,
                ServiceName = service.Name,
                UnitPrice = service.Price,
                Quantity = request.Quantity
            }, ct);
            await _uow.SaveChangesAsync(ct);

            return await BuildBillAsync(r, ct);
        }, ct);
    }

    public async Task<BillResponse> RemoveExtraAsync(int reservationId, int extraId, CancellationToken ct = default)
    {
        return await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _locker.LockAsync(reservationId, ct);

            var r = await GetAccessibleAsync(reservationId, ct);

            if (!ExtrasAllowed.Contains(r.Status))
                throw new DomainException($"Cannot change extras of a reservation that is {r.Status}.");

         
            if (!IsFrontDesk && r.Status == ReservationStatus.CheckedIn)
                throw new DomainException("Please contact the front desk to change extras during your stay.");

            
            var line = await _extras.FirstOrDefaultAsync(e => e.Id == extraId && e.ReservationId == reservationId, ct)
                ?? throw new NotFoundException(nameof(ReservationExtra), extraId);

            var before = await BuildBillAsync(r, ct);
            if (before.GrandTotal - line.Total < before.NetPaid)
                throw new DomainException("Removing this item would leave the reservation overpaid. Issue a refund first.");

            _extras.Remove(line);   
            await _uow.SaveChangesAsync(ct);

            return await BuildBillAsync(r, ct);
        }, ct);
    }



    public async Task<BillResponse> PayAsync(int reservationId, CreatePaymentRequest request, CancellationToken ct = default)
    {
        await _paymentValidator.ValidateAndThrowAsync(request, ct);

  
        if (!IsFrontDesk && request.Method != PaymentMethod.Card)
            throw new DomainException("Online payments must use Card. Cash and bank transfers are recorded by the front desk.");

        return await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _locker.LockAsync(reservationId, ct);

            var r = await GetAccessibleAsync(reservationId, ct);

            if (!PaymentsAllowed.Contains(r.Status))
                throw new DomainException($"Cannot accept payments for a reservation that is {r.Status}.");

            var before = await BuildBillAsync(r, ct);

          
            if (before.Balance <= 0)
                throw new DomainException("This reservation is already fully paid.");
            if (request.Amount > before.Balance)
                throw new DomainException($"Amount exceeds the remaining balance ({before.Balance:0.00}).");

            await _payments.AddAsync(new Payment
            {
                Reference = NewReference(),
                Amount = request.Amount,
                Type = PaymentType.Payment,
                Method = request.Method,
                Notes = request.Notes?.Trim(),
                CreatedByUserId = _currentUser.UserId,
                ReservationId = r.Id
            }, ct);

           
            var newNet = before.NetPaid + request.Amount;
            var deposit = decimal.Round(before.GrandTotal * DepositRate, 2);
            if (r.Status == ReservationStatus.Pending && newNet >= deposit)
                r.Status = ReservationStatus.Confirmed;

            await _uow.SaveChangesAsync(ct);

            return await BuildBillAsync(r, ct);
        }, ct);
    }

    public async Task<BillResponse> RefundAsync(int reservationId, RefundRequest request, CancellationToken ct = default)
    {
        await _refundValidator.ValidateAndThrowAsync(request, ct);

        return await _uow.ExecuteInTransactionAsync(async () =>
        {
            await _locker.LockAsync(reservationId, ct);

            var r = await GetAccessibleAsync(reservationId, ct);
            var before = await BuildBillAsync(r, ct);

        
            if (request.Amount > before.NetPaid)
                throw new DomainException($"Refund exceeds the net paid amount ({before.NetPaid:0.00}).");

            await _payments.AddAsync(new Payment
            {
                Reference = NewReference(),
                Amount = request.Amount,
                Type = PaymentType.Refund,
                Method = request.Method,
                Notes = request.Notes?.Trim(),
                CreatedByUserId = _currentUser.UserId,
                ReservationId = r.Id
            }, ct);

            await _uow.SaveChangesAsync(ct);

            return await BuildBillAsync(r, ct);
        }, ct);
    }

   
    private async Task<Reservation> GetAccessibleAsync(int id, CancellationToken ct)
    {
        var r = await _reservations.GetByIdAsync(id, ct, x => x.Customer)
            ?? throw new NotFoundException(nameof(Reservation), id);

      
        if (!IsFrontDesk && (_currentUser.UserId is null || r.Customer.UserId != _currentUser.UserId))
            throw new NotFoundException(nameof(Reservation), id);

        return r;
    }


    private async Task<BillResponse> BuildBillAsync(Reservation r, CancellationToken ct)
    {
        var extras = await _extras.ListAsync(e => e.ReservationId == r.Id, ct);
        var payments = await _payments.ListAsync(p => p.ReservationId == r.Id, ct);
        return BillCalculator.Calculate(r, extras, payments);
    }

   
    private static string NewReference()
        => "PAY-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
}