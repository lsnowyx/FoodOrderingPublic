using Application.DTOs.Ingredient;
using Application.Features.Ingredient.Create;
using Application.Features.Ingredient.Update;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class IngredientMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateIngredientRequest, CreateIngredientCommand>()
            .Map(dest => dest.UnitCost, src => src.UnitCost!.Value);
        config.NewConfig<UpdateIngredientRequest, UpdateIngredientCommand>()
            // Set from the route by IngredientsController.
            .Ignore(dest => dest.Id)
            .Map(dest => dest.UnitCost, src => src.UnitCost!.Value);

        config.NewConfig<CreateIngredientCommand, Ingredient>()
            // Id is generated and relationships are managed by EF.
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.MenuItemIngredients);

        config.NewConfig<Ingredient, IngredientResponse>();
        config.NewConfig<Ingredient, IngredientLookupResponse>()
            .Map(dest => dest.Text, src => src.Name);
    }
}
