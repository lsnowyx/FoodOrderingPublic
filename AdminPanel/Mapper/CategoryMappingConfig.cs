using AdminPanel.Models.Category;
using AdminPanel.Models.Common;
using Application.DTOs.Category;
using Application.DTOs.Common;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace AdminPanel.Mapper;

public sealed class CategoryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CategoryViewModel, CreateCategoryRequest>();
        config.NewConfig<CategoryViewModel, UpdateCategoryRequest>();

        config.NewConfig<CategoryResponse, CategoryViewModel>()
            .Map(destination => destination.ImageFile, _ => (IFormFile?)null);

        config.NewConfig<PaginatedResponse<CategoryResponse>, PaginatedViewModel<CategoryViewModel>>();
    }
}
