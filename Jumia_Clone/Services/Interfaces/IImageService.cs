using Jumia_Clone.Models.Enums;

namespace Jumia_Clone.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile, EntityType entityType, string name);
        Task<bool> DeleteImageAsync(string imagePath);
        Task<string> UpdateImageAsync(IFormFile newImageFile, string oldImagePath, EntityType entityType, string name);
        string GetImageUrl(string relativePath);
    }
}
