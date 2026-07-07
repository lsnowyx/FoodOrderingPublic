namespace Domain.ConfigModels;

public class DeliverySimulationSettings
{
    public decimal RestaurantLatitude { get; set; }

    public decimal RestaurantLongitude { get; set; }

    public int DefaultDurationSeconds { get; set; } = 180;

    public int UpdateIntervalSeconds { get; set; } = 5;
}
