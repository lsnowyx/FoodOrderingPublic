using Application.DTOs.Cloudinary;
using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Services;

public interface ICloudinaryService
{
    Task<CloudinaryImageUploadResponse> AddImageAsync(IFormFile file, CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}
