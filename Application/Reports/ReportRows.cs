
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reports;


public record StayRow(DateOnly CheckIn, DateOnly CheckOut, decimal TotalPrice);


public record PaymentAggregateRow(DateOnly Date, PaymentType Type, PaymentMethod Method, decimal Amount);


public record ExtraAggregateRow(string ServiceName, int Quantity, decimal Revenue);