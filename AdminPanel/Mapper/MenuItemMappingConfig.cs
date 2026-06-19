using AdminPanel.Models.Category;
using AdminPanel.Models.Common;
using AdminPanel.Models.MenuItem;
using Application.DTOs.Category;
using Application.DTOs.Common;
using Application.DTOs.MenuItem;
using Application.DTOs.MenuItemIngredient;
using Application.DTOs.MenuItemPicture;
using Application.Features.MenuItemPicture.Update;
using Mapster;

namespace AdminPanel.Mapper;

public sealed class MenuItemMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MenuItemResponse, MenuItemViewModel>()
            .Ignore(destination => destination.Pictures)
            .Map(destination => destination.CategoryName, source => source.CategoryName);

        config.NewConfig<PaginatedResponse<MenuItemResponse>, PaginatedViewModel<MenuItemViewModel>>()
            .Map(destination => destination.Page, source => source.Page)
            .Map(destination => destination.PageSize, source => source.PageSize)
            .Map(destination => destination.TotalCount, source => source.TotalCount)
            .Map(destination => destination.TotalPages, source => source.TotalPages)
            .Map(destination => destination.Items, source => source.Items);

        config.NewConfig<MenuItemDetailsResponse, MenuItemViewModel>()
            .Map(destination => destination.Pictures, source => source.Pictures)
            .Map(destination => destination.CategoryName, source => source.CategoryName);

        config.NewConfig<MenuItemDetailsResponse, MenuItemDetailsViewModel>()
            .Map(destination => destination.Item, source => source)
            .Map(destination => destination.Ingredients, source => source.Ingredients)
            .Map(destination => destination.Pictures, source => source.Pictures);

        config.NewConfig<MenuItemViewModel, CreateMenuItemRequest>();
        config.NewConfig<MenuItemViewModel, UpdateMenuItemRequest>();

        config.NewConfig<MenuItemIngredientResponse, MenuItemIngredientViewModel>();

        config.NewConfig<MenuItemPictureResponse, MenuItemPictureViewModel>()
            .Map(destination => destination.ImageFile, _ => (IFormFile?)null);

        config.NewConfig<MenuItemPictureViewModel, MenuItemPictureRequest>()
            .Map(destination => destination.ImageFile, source => source.ImageFile!);

        config.NewConfig<MenuItemPictureRequest, UpdateMenuItemPictureCommand>()
            .Ignore(destination => destination.MenuItemId)
            .Ignore(destination => destination.PictureId);

        config.NewConfig<CategoryLookupResponse, CategoryViewModel>()
            .Map(destination => destination.Name, source => source.Text)
            .Map(destination => destination.ImageUrl, _ => (string?)null)
            .Map(destination => destination.ImageFile, _ => (IFormFile?)null);
    }
}
