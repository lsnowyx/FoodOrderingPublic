using Application.DTOs.Catalog;
using Application.DTOs.Category;
using Application.Features.Category.Create;
using Application.Features.Category.Update;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCategoryRequest, CreateCategoryCommand>();
        config.NewConfig<UpdateCategoryRequest, UpdateCategoryCommand>()
            // Set from the route by CategoriesController.
            .Ignore(dest => dest.Id);

        config.NewConfig<CreateCategoryCommand, Category>()
            // Id is generated; Cloudinary fields and child menu items are set outside the request mapping.
            .Ignore(dest => dest.Id)
            .Map(dest => dest.ImageUrl, _ => (string?)null)
            .Map(dest => dest.ImagePublicId, _ => (string?)null)
            .Ignore(dest => dest.MenuItems);

        config.NewConfig<Category, CategoryResponse>();
        config.NewConfig<Category, CategoryLookupResponse>()
            .Map(dest => dest.Text, src => src.Name);
        config.NewConfig<Category, CatalogCategoryResponse>();
        config.NewConfig<Category, CatalogCategorySummaryResponse>();
    }
}
