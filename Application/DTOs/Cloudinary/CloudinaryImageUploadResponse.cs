namespace Application.DTOs.Cloudinary;

public class CloudinaryImageUploadResponse
{
    public string PublicId { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string Format { get; set; } = null!;
    public int Width { get; set; }
    public int Height { get; set; }
}