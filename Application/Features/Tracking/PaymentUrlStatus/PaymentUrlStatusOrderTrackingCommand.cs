using Application.DTOs.Payment;
using MediatR;

namespace Application.Features.Tracking.PaymentUrlStatus;

public sealed record PaymentUrlStatusOrderTrackingCommand(string TrackingToken) : IRequest<PaymentUrlStatusResult>;