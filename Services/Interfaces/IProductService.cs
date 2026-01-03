using EasyOrderCs.Dtos.Product;
using EasyOrderCs.Models;

namespace EasyOrderCs.Services.Interfaces;

public interface IProductService
{
    Task<Product> CreateAsync(CreateProductDto createProductDto);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByEnterpriseAsync(Guid enterpriseId);
    Task<Product> GetByIdAsync(Guid id);
    Task<Product> UpdateAsync(Guid id, UpdateProductDto updateProductDto);
    Task<Product> DeleteAsync(Guid id);
    Task<Product> UploadPhotoAsync(Guid id, IFormFile file);
    Task<Product> UpdateStockAsync(Guid id, int quantity);
    Task ValidateStockAvailabilityAsync(Guid productId, int requestedQuantity);
    Task ReserveStockAsync(Guid productId, int quantity);
    Task ReleaseStockAsync(Guid productId, int quantity);
}

