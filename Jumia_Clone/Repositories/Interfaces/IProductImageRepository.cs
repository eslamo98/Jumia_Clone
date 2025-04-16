using Jumia_Clone.Models.DTOs.ProductImageDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IProductImageRepository
    {
        Task<IEnumerable<ProductImageDto>> AddProductImagesAsync(CreateProductImageDto imageDto);
        Task UpdateProductImagesAsync(UpdateProductImagesDto imageDto);
        Task DeleteProductImagesAsync(List<int> imageIds);
    }
}
