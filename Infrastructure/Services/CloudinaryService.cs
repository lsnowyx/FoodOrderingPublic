using Application.Abstractions.Services;
using Application.DTOs.Cloudinary;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain.ConfigModels;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    private static readonly string[] AllowedImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    public CloudinaryService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;

        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret);

        _cloudinary = new Cloudinary(account)
        {
            Api =
            {
                Secure = true
            }
        };
    }

    public async Task<CloudinaryImageUploadResponse> AddImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("Image file is required.", nameof(file));

        ValidateImage(file);

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = CloudinaryFolderConstants.Images,
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error is not null)
            throw new InvalidOperationException(uploadResult.Error.Message);

        return new CloudinaryImageUploadResponse
        {
            PublicId = uploadResult.PublicId,
            ImageUrl = uploadResult.SecureUrl.ToString(),
            Format = uploadResult.Format,
            Width = uploadResult.Width,
            Height = uploadResult.Height
        };
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("Public ID is required.", nameof(publicId));

        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image,
            Invalidate = true
        };

        var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

        return deleteResult.Result switch
        {
            "ok" => true,
            "not found" => true,
            _ => false
        };
    }

    private static void ValidateImage(IFormFile file)
    {
        const long maxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        if (file.Length > maxFileSizeInBytes)
            throw new ArgumentException("Maximum image size is 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedImageExtensions.Contains(extension))
            throw new ArgumentException("Only JPG, JPEG, PNG, and WEBP images are allowed.");

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Uploaded file must be an image.");
    }
}
