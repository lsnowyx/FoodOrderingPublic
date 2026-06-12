using System.Net;
using AdminPanel.Models.Category;
using AdminPanel.Models.MenuItem;
using AdminPanel.Models.Order;
using Microsoft.AspNetCore.Http;

namespace AdminPanel.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string url);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data);
    Task<bool> DeleteAsync(string url);
    Task<TResponse?> PatchAsync<TRequest, TResponse>(string url, TRequest data);
    Task<IEnumerable<OrderViewModel>?> GetAvailableOrdersAsync();
    Task<OrderViewModel?> TakeOrderAsync(Guid orderId);
    Task<OrderViewModel?> GetMyDeliveryAsync();
    Task StartDeliveryAsync(Guid orderId);
    Task MarkDeliveredAsync(Guid orderId);
    Task<CategoryViewModel?> CreateCategoryAsync(CategoryViewModel model);
    Task<CategoryViewModel?> UpdateCategoryAsync(Guid categoryId, CategoryViewModel model);
    Task<MenuItemPictureViewModel?> UploadMenuItemPictureAsync(Guid menuItemId, IFormFile imageFile, string? caption);
    Task<MenuItemPictureViewModel?> UpdateMenuItemPictureAsync(Guid menuItemId, Guid pictureId, IFormFile imageFile, string? caption);
}
