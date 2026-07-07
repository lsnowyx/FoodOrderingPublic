using Application.DTOs.DeliveryTracking;

namespace Application.Abstractions.Services;

public interface IDeliveryLocationCalculator
{
    DeliveryLocationSnapshot Calculate(DeliveryLocationCalculationRequest request);
}
