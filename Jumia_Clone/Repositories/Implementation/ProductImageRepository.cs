using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.ProductImageDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Models.Enums;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IProductRepository _productRepository;

        public ProductImageRepository(
            ApplicationDbContext context,
            IImageService imageService,
            IProductRepository productRepository)
        {
            _context = context;
            _imageService = imageService;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductImageDto>> AddProductImagesAsync(CreateProductImageDto imageDto)
        {
            // Validate product exists
            var product = await _context.Products.FindAsync(imageDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {imageDto.ProductId} not found");

            var savedImages = new List<ProductImageDto>();

            var existingImages = await _context.ProductImages
        .Where(pi => pi.ProductId == imageDto.ProductId)
        .ToListAsync();

            int maxDisplayOrder = existingImages.Any()
                ? existingImages.Max(pi => pi.DisplayOrder ?? 0)
                : 0;

            foreach (var imageFile in imageDto.ImageFiles)
            {
                string entityName = $"{product.Name}-{product.ProductId}";

                // Save image using ImageService
                string imagePath = await _imageService.SaveImageAsync(
                    imageFile,
                    EntityType.Product,
                    entityName
                );

                // Create ProductImage entity
                var productImage = new ProductImage
                {
                    ProductId = imageDto.ProductId,
                    ImageUrl = imagePath,
                    DisplayOrder = maxDisplayOrder + 1
                };

                _context.ProductImages.Add(productImage);

                // Increment display order for next image
                maxDisplayOrder++;

                // Map to DTO
                savedImages.Add(new ProductImageDto
                {
                    ImageId = productImage.ImageId,
                    ProductId = productImage.ProductId,
                    ImageUrl = productImage.ImageUrl,
                    DisplayOrder = productImage.DisplayOrder ?? 0
                });
            }

            await _context.SaveChangesAsync();

            return savedImages;
        }

        public async Task UpdateProductImagesAsync(UpdateProductImagesDto imageDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // First, delete specified images
                if (imageDto.ImagesToDelete != null && imageDto.ImagesToDelete.Any())
                {
                    await DeleteProductImagesAsync(imageDto.ImagesToDelete);
                }

                // Then add new images
                if (imageDto.NewImageFiles != null && imageDto.NewImageFiles.Any())
                {
                    await AddProductImagesAsync(new CreateProductImageDto
                    {
                        ProductId = imageDto.ProductId,
                        ImageFiles = imageDto.NewImageFiles
                    });
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteProductImagesAsync(List<int> imageIds)
        {
            if (imageIds == null || !imageIds.Any())
                return;

            // Find images to delete
            var imagesToDelete = await _context.ProductImages
                .Where(pi => imageIds.Contains(pi.ImageId))
                .ToListAsync();

            foreach (var image in imagesToDelete)
            {
                // Delete physical file
                await _imageService.DeleteImageAsync(image.ImageUrl);

                // Remove from database
                _context.ProductImages.Remove(image);
            }

            await _context.SaveChangesAsync();
        }
    }
}
