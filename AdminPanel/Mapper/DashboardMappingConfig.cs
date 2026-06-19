using AdminPanel.Models.Dashboard;
using Application.DTOs.Dashboard;
using Mapster;

namespace AdminPanel.Mapper;

public sealed class DashboardMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DashboardOrderSummaryResponse, OrderSummaryViewModel>();

        config.NewConfig<DashboardSummaryResponse, DashboardViewModel>()
            .Map(destination => destination.IsMenuManager, _ => false)
            .Map(destination => destination.IsOrderManager, _ => false)
            .Map(destination => destination.ShowOrderSummary, _ => false)
            .Map(destination => destination.UserName, _ => (string?)null);
    }
}
