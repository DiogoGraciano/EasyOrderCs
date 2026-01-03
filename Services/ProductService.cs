using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using EasyOrderCs.Data;
using EasyOrderCs.Models;
using EasyOrderCs.Dtos.Product;
using EasyOrderCs.Services.Interfaces;

namespace EasyOrderCs.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public ProductService(ApplicationDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    public async Task<Product> CreateAsync(CreateProductDto createProductDto)
    {
        await ValidateProductCreationAsync(createProductDto);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = createProductDto.Name.Trim(),
            Description = createProductDto.Description.Trim(),
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
            Photo = createProductDto.Photo,
            EnterpriseId = createProductDto.EnterpriseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }

    private async Task ValidateProductCreationAsync(CreateProductDto createProductDto)
    {
        ValidateBasicData(createProductDto);
        ValidatePrice(createProductDto.Price);
        ValidateStock(createProductDto.Stock);
        await ValidateEnterpriseExistsAsync(createProductDto.EnterpriseId);
        await ValidateNameUniquenessAsync(createProductDto.Name, createProductDto.EnterpriseId);
    }

    private void ValidateBasicData(CreateProductDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var trimmed = dto.Name.Trim();
            if (trimmed.Length < 2)
            {
                throw new ArgumentException("O nome do produto deve ter pelo menos 2 caracteres");
            }

            if (trimmed.Length > 255)
            {
                throw new ArgumentException("O nome do produto não pode ter mais de 255 caracteres");
            }

            var namePattern = new Regex(@"^[a-zA-ZÀ-ÿ0-9\s\-_.()]+$");
            if (!namePattern.IsMatch(trimmed))
            {
                throw new ArgumentException("O nome do produto contém caracteres inválidos");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            var trimmed = dto.Description.Trim();
            if (trimmed.Length < 10)
            {
                throw new ArgumentException("A descrição deve ter pelo menos 10 caracteres");
            }

            if (trimmed.Length > 1000)
            {
                throw new ArgumentException("A descrição não pode ter mais de 1000 caracteres");
            }
        }
    }

    private void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("O preço não pode ser negativo");
        }

        if (price == 0)
        {
            throw new ArgumentException("O preço deve ser maior que zero");
        }

        if (price > 1000000)
        {
            throw new ArgumentException("O preço não pode ser superior a R$ 1.000.000,00");
        }

        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(price)[3])[2];
        if (decimalPlaces > 2)
        {
            throw new ArgumentException("O preço deve ter no máximo 2 casas decimais");
        }
    }

    private void ValidateStock(int stock)
    {
        if (stock < 0)
        {
            throw new ArgumentException("O estoque não pode ser negativo");
        }

        if (stock > 999999)
        {
            throw new ArgumentException("O estoque não pode ser superior a 999.999 unidades");
        }
    }

    private async Task ValidateEnterpriseExistsAsync(Guid enterpriseId)
    {
        if (enterpriseId == Guid.Empty)
            return;

        var enterprise = await _context.Enterprises.FindAsync(enterpriseId);
        if (enterprise == null)
        {
            throw new KeyNotFoundException($"Empresa com ID {enterpriseId} não encontrada");
        }
    }

    private async Task ValidateNameUniquenessAsync(string name, Guid enterpriseId, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || enterpriseId == Guid.Empty)
            return;

        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Name == name.Trim() && p.EnterpriseId == enterpriseId);

        if (existingProduct != null && existingProduct.Id != excludeId)
        {
            throw new InvalidOperationException("Já existe um produto com este nome nesta empresa");
        }
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Enterprise)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByEnterpriseAsync(Guid enterpriseId)
    {
        if (enterpriseId == Guid.Empty)
        {
            throw new ArgumentException("ID da empresa é obrigatório");
        }

        await ValidateEnterpriseExistsAsync(enterpriseId);

        return await _context.Products
            .Where(p => p.EnterpriseId == enterpriseId)
            .Include(p => p.Enterprise)
            .ToListAsync();
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID do produto é obrigatório");
        }

        var product = await _context.Products
            .Include(p => p.Enterprise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            throw new KeyNotFoundException($"Produto com ID {id} não encontrado");
        }

        return product;
    }

    public async Task<Product> UpdateAsync(Guid id, UpdateProductDto updateProductDto)
    {
        var product = await GetByIdAsync(id);
        await ValidateProductUpdateAsync(product, updateProductDto);

        if (!string.IsNullOrWhiteSpace(updateProductDto.Name))
        {
            product.Name = updateProductDto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(updateProductDto.Description))
        {
            product.Description = updateProductDto.Description.Trim();
        }

        if (updateProductDto.Price.HasValue)
        {
            product.Price = updateProductDto.Price.Value;
        }

        if (updateProductDto.Stock.HasValue)
        {
            product.Stock = updateProductDto.Stock.Value;
        }

        if (updateProductDto.Photo != null)
        {
            product.Photo = updateProductDto.Photo;
        }

        if (updateProductDto.EnterpriseId.HasValue)
        {
            product.EnterpriseId = updateProductDto.EnterpriseId.Value;
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private async Task ValidateProductUpdateAsync(Product existingProduct, UpdateProductDto updateProductDto)
    {
        if (!string.IsNullOrWhiteSpace(updateProductDto.Name))
        {
            var trimmed = updateProductDto.Name.Trim();
            if (trimmed.Length < 2)
            {
                throw new ArgumentException("O nome do produto deve ter pelo menos 2 caracteres");
            }

            if (trimmed.Length > 255)
            {
                throw new ArgumentException("O nome do produto não pode ter mais de 255 caracteres");
            }

            var namePattern = new Regex(@"^[a-zA-ZÀ-ÿ0-9\s\-_.()]+$");
            if (!namePattern.IsMatch(trimmed))
            {
                throw new ArgumentException("O nome do produto contém caracteres inválidos");
            }
        }

        if (!string.IsNullOrWhiteSpace(updateProductDto.Description))
        {
            var trimmed = updateProductDto.Description.Trim();
            if (trimmed.Length < 10)
            {
                throw new ArgumentException("A descrição deve ter pelo menos 10 caracteres");
            }

            if (trimmed.Length > 1000)
            {
                throw new ArgumentException("A descrição não pode ter mais de 1000 caracteres");
            }
        }

        if (updateProductDto.Price.HasValue)
        {
            ValidatePrice(updateProductDto.Price.Value);
        }

        if (updateProductDto.Stock.HasValue)
        {
            ValidateStock(updateProductDto.Stock.Value);
        }

        if (updateProductDto.EnterpriseId.HasValue && updateProductDto.EnterpriseId.Value != existingProduct.EnterpriseId)
        {
            await ValidateEnterpriseExistsAsync(updateProductDto.EnterpriseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(updateProductDto.Name) && updateProductDto.Name.Trim() != existingProduct.Name)
        {
            var enterpriseId = updateProductDto.EnterpriseId ?? existingProduct.EnterpriseId;
            await ValidateNameUniquenessAsync(updateProductDto.Name, enterpriseId, existingProduct.Id);
        }
    }

    public async Task<Product> DeleteAsync(Guid id)
    {
        var product = await GetByIdAsync(id);

        if (!string.IsNullOrEmpty(product.Photo))
        {
            await _fileUploadService.DeleteFileAsync(product.Photo);
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<Product> UploadPhotoAsync(Guid id, IFormFile file)
    {
        var product = await GetByIdAsync(id);
        ValidatePhotoFile(file);

        if (!string.IsNullOrEmpty(product.Photo))
        {
            await _fileUploadService.DeleteFileAsync(product.Photo);
        }

        var photoUrl = await _fileUploadService.UploadFileAsync(file, "products");
        product.Photo = photoUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return product;
    }

    private void ValidatePhotoFile(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("Tipo de arquivo inválido. Permitidos: JPEG, PNG, WebP");
        }

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSize)
        {
            throw new ArgumentException("Arquivo muito grande. Máximo: 5MB");
        }

        if (file.FileName.Length > 255)
        {
            throw new ArgumentException("Nome do arquivo muito longo. Máximo: 255 caracteres");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("Extensão de arquivo inválida. Permitidas: .jpg, .jpeg, .png, .webp");
        }
    }

    public async Task<Product> UpdateStockAsync(Guid id, int quantity)
    {
        var product = await GetByIdAsync(id);

        if (quantity == 0)
        {
            throw new ArgumentException("A quantidade deve ser diferente de zero");
        }

        var newStock = product.Stock + quantity;

        if (newStock < 0)
        {
            throw new ArgumentException(
                $"Estoque insuficiente. Estoque atual: {product.Stock}, Tentativa de redução: {Math.Abs(quantity)}");
        }

        ValidateStock(newStock);

        product.Stock = newStock;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task ValidateStockAvailabilityAsync(Guid productId, int requestedQuantity)
    {
        var product = await GetByIdAsync(productId);

        if (product.Stock < requestedQuantity)
        {
            throw new ArgumentException(
                $"Estoque insuficiente para o produto {product.Name}. " +
                $"Disponível: {product.Stock}, Solicitado: {requestedQuantity}");
        }
    }

    public async Task ReserveStockAsync(Guid productId, int quantity)
    {
        await ValidateStockAvailabilityAsync(productId, quantity);
        await UpdateStockAsync(productId, -quantity);
    }

    public async Task ReleaseStockAsync(Guid productId, int quantity)
    {
        await UpdateStockAsync(productId, quantity);
    }
}

