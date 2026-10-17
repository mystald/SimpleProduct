using SimpleProduct.Server.Products.Dtos;

namespace SimpleProduct.Server.Products.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductResponseDto>> SearchAsync(ProductSearchDto searchDto, CancellationToken cancellationToken = default);
}
