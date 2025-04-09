using Jumia_Clone.Models.Enums;
using Jumia_Clone.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Jumia_Clone.Services.Implementation
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _imagesFolder = "Images";

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, EntityType entityType, string name)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("No image file provided");
            }

            // Create directory structure if it doesn't exist
            // First level: entity type folder (e.g., Products, Categories)
            string entityTypeFolder = entityType.ToString() + "s"; // Add 's' to make it plural

            // Second level: specific entity folder (e.g., product-name)
            string sanitizedEntityName = SanitizeFileName(name);

            // Combined path: Images/Products/product-name/
            string directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, _imagesFolder, entityTypeFolder, sanitizedEntityName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Create filename using first 10 chars of original filename + GUID + extension
            string fileName = GenerateUniqueFileName(imageFile.FileName, Path.GetExtension(imageFile.FileName));
            string filePath = Path.Combine(directoryPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Return relative path from wwwroot
            return Path.Combine(_imagesFolder, entityTypeFolder, sanitizedEntityName, fileName);
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return false;
            }

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                return true;
            }

            return false;
        }

        public async Task<string> UpdateImageAsync(IFormFile newImageFile, string oldImagePath, EntityType entityType, string name)
        {
            // Delete old image if it exists
            if (!string.IsNullOrEmpty(oldImagePath))
            {
                await DeleteImageAsync(oldImagePath);
            }

            // Save new image
            return await SaveImageAsync(newImageFile, entityType, name);
        }

        public string GetImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            // Convert backslashes to forward slashes for URL format
            return relativePath.Replace("\\", "/");
        }

        private string GenerateUniqueFileName(string originalFileName, string extension)
        {
            // Take up to first 10 chars of the original filename (without extension)
            string baseFileName = Path.GetFileNameWithoutExtension(originalFileName);
            string truncatedName = baseFileName.Length > 10 ? baseFileName.Substring(0, 10) : baseFileName;

            // Sanitize the truncated name
            truncatedName = SanitizeFileName(truncatedName);

            // Add GUID to ensure uniqueness
            string guid = Guid.NewGuid().ToString("N").Substring(0, 8); // Using first 8 chars of GUID for brevity

            return $"{truncatedName}_{guid}{extension}";
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove invalid file name characters
            string invalidChars = new string(Path.GetInvalidFileNameChars());
            string sanitized = fileName;

            foreach (char c in invalidChars)
            {
                sanitized = sanitized.Replace(c.ToString(), "");
            }

            // Replace spaces with hyphens
            sanitized = sanitized.Replace(" ", "-");

            // Limit length to avoid potential path length issues
            int maxLength = 50;
            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength);
            }

            return sanitized.ToLower();
        }
    }
}