using Application.DTOs.Catalog;
using Application.DTOs.MenuItem;
using Application.DTOs.MenuItemIngredient;
using Application.DTOs.MenuItemPicture;
using Application.Features.MenuItem.Create;
using Application.Features.MenuItem.Update;
using Application.Features.MenuItemIngredient.Create;
using Application.Features.MenuItemIngredient.Update;
using Application.Features.MenuItemPicture.Create;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class MenuMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateMenuItemRequest, CreateMenuItemCommand>();
        config.NewConfig<UpdateMenuItemRequest, UpdateMenuItemCommand>()
            // Set from the route by MenuItemsController.
            .Ignore(dest => dest.Id);

        config.NewConfig<CreateMenuItemCommand, MenuItem>()
            // Id is generated and relationships are managed through separate EF associations.
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Category)
            .Map(dest => dest.RestaurantCost, _ => 0m)
            .Map(dest => dest.TotalCalories, _ => 0m)
            .Ignore(dest => dest.MenuItemIngredients)
            .Ignore(dest => dest.MenuItemPictures);

        config.NewConfig<MenuItemPictureRequest, CreateMenuItemPictureCommand>()
            // Set from the route by MenuItemPicturesController.
            .Ignore(dest => dest.MenuItemId);

        config.NewConfig<CreateMenuItemIngredientRequest, CreateMenuItemIngredientCommand>()
            // Set from the route by MenuItemIngredientsController.
            .Ignore(dest => dest.MenuItemId)
            .Map(dest => dest.Quantity, src => src.Quantity!.Value);

        config.NewConfig<UpdateMenuItemIngredientRequest, UpdateMenuItemIngredientCommand>()
            .Ignore(dest => dest.MenuItemId)
            .Ignore(dest => dest.IngredientId)
            .Map(dest => dest.Quantity, src => src.Quantity!.Value);

        config.NewConfig<MenuItemPicture, MenuItemPictureResponse>();
        config.NewConfig<MenuItemPicture, CatalogMenuItemPictureResponse>();

        config.NewConfig<MenuItemIngredient, MenuItemIngredientResponse>()
            .Map(dest => dest.IngredientName, src => src.Ingredient != null ? src.Ingredient.Name : string.Empty)
            .Map(dest => dest.BaseUnit, src => src.Ingredient != null ? src.Ingredient.BaseUnit : string.Empty)
            .Map(dest => dest.UnitCost, src => src.Ingredient != null ? src.Ingredient.UnitCost : 0m)
            .Map(dest => dest.AllergenInfo, src => src.Ingredient != null ? src.Ingredient.AllergenInfo : null)
            .Map(dest => dest.CaloriesPerUnit, src => src.Ingredient != null ? src.Ingredient.CaloriesPerUnit : null)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(
                dest => dest.LineCost,
                src => src.Ingredient != null
                    ? src.Ingredient.UnitCost * src.Quantity
                    : 0m)
            .Map(
                dest => dest.LineCalories,
                src => src.Ingredient != null
                    ? (src.Ingredient.CaloriesPerUnit ?? 0) * src.Quantity
                    : 0m);

        config.NewConfig<MenuItemIngredient, CatalogMenuItemIngredientResponse>()
            .Map(dest => dest.Name, src => src.Ingredient.Name)
            .Map(dest => dest.AllergenInfo, src => src.Ingredient.AllergenInfo);

        config.NewConfig<MenuItem, MenuItemResponse>()
            .Map(
                dest => dest.CategoryName,
                src => src.Category != null ? src.Category.Name : string.Empty);

        config.NewConfig<MenuItem, MenuItemDetailsResponse>()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.Ingredients, src => src.MenuItemIngredients)
            .Map(dest => dest.Pictures, src => src.MenuItemPictures);

        config.NewConfig<MenuItem, CatalogMenuItemResponse>()
            .Map(dest => dest.DisplayPictureUrl, src => src.MenuItemPictures
                .OrderBy(picture => picture.Id)
                .Select(picture => picture.ImageUrl)
                .FirstOrDefault());

        config.NewConfig<MenuItem, CatalogMenuItemDetailsResponse>()
            .Map(dest => dest.Category, src => src.Category)
            .Map(dest => dest.Pictures, src => src.MenuItemPictures.OrderBy(picture => picture.Id))
            .Map(dest => dest.Ingredients, src => src.MenuItemIngredients.OrderBy(menuItemIngredient => menuItemIngredient.Ingredient.Name));
    }
}
