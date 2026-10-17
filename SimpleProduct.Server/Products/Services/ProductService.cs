using Microsoft.EntityFrameworkCore;
using SimpleProduct.Server.Data;
using SimpleProduct.Server.Products.Dtos;
using SimpleProduct.Server.Products.Models;

namespace SimpleProduct.Server.Products.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all products");
        return await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToResponseDto(p))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product == null ? null : MapToResponseDto(product);
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new product with name: {ProductName}", dto.Name);
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Price = dto.Price,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
        return MapToResponseDto(product);
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for update", id);
            return null;
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.Price = dto.Price;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product with ID {ProductId} updated successfully", id);

        return MapToResponseDto(product);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);

        return true;
    }

    public async Task<IEnumerable<ProductResponseDto>> SearchAsync(ProductSearchDto searchDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching products with criteria: Name={Name}, MinPrice={MinPrice}, MaxPrice={MaxPrice}",
            searchDto.Name, searchDto.MinPrice, searchDto.MaxPrice);

        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchDto.Name))
        {
            var trimmedName = searchDto.Name.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{trimmedName}%"));
        }

        if (searchDto.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
        }

        if (searchDto.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToResponseDto(p))
            .ToListAsync(cancellationToken);
    }

    private static ProductResponseDto MapToResponseDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        CreatedAt = p.CreatedAt
    };
}
