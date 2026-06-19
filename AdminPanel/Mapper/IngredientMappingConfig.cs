using AdminPanel.Models.Common;
using AdminPanel.Models.Ingredient;
using Application.DTOs.Common;
using Application.DTOs.Ingredient;
using Mapster;

namespace AdminPanel.Mapper;

public sealed class IngredientMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<decimal, decimal?>()
            .MapWith(source => (decimal?)source);

        config.NewConfig<IngredientViewModel, CreateIngredientRequest>();
        config.NewConfig<IngredientViewModel, UpdateIngredientRequest>();
        config.NewConfig<IngredientResponse, IngredientViewModel>()
            .Map(destination => destination.UnitCost, source => (decimal?)source.UnitCost);
        config.NewConfig<PaginatedResponse<IngredientResponse>, PaginatedViewModel<IngredientViewModel>>();
    }
}
