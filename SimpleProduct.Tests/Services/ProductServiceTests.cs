using Microsoft.Extensions.Logging;
using Moq;
using SimpleProduct.Server.Data;
using SimpleProduct.Server.Products.Dtos;
using SimpleProduct.Server.Products.Models;
using SimpleProduct.Server.Products.Services;
using SimpleProduct.Tests.Helpers;
using Xunit;

namespace SimpleProduct.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<ILogger<ProductService>> _loggerMock = new();

    private (ProductService service, ApplicationDbContext context) CreateService(string dbName)
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext(dbName);
        var service = new ProductService(context, _loggerMock.Object);
        return (service, context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts_OrderedByCreatedAtDescending()
    {
        // Arrange
        var (service, context) = CreateService(nameof(GetAllAsync_ReturnsAllProducts_OrderedByCreatedAtDescending));
        var p1 = new Product { Name = "First", Price = 10, CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
        var p2 = new Product { Name = "Second", Price = 20, CreatedAt = DateTime.UtcNow };
        context.Products.AddRange(p1, p2);
        await context.SaveChangesAsync();

        // Act
        var result = (await service.GetAllAsync(CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Second", result[0].Name);
        Assert.Equal("First", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenIdExists()
    {
        // Arrange
        var (service, context) = CreateService(nameof(GetByIdAsync_ReturnsProduct_WhenIdExists));
        var product = new Product { Name = "Keyboard", Description = "Mechanical", Price = 75.50m };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(product.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Keyboard", result.Name);
        Assert.Equal("Mechanical", result.Description);
        Assert.Equal(75.50m, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenIdNotFound()
    {
        // Arrange
        var (service, _) = CreateService(nameof(GetByIdAsync_ReturnsNull_WhenIdNotFound));

        // Act
        var result = await service.GetByIdAsync(9999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsProductToDatabase_AndReturnsDto()
    {
        // Arrange
        var (service, context) = CreateService(nameof(CreateAsync_AddsProductToDatabase_AndReturnsDto));
        var dto = new CreateProductDto
        {
            Name = "  Gaming Mouse  ",
            Description = "  Ergonomic RGB mouse  ",
            Price = 49.99m
        };

        // Act
        var result = await service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Gaming Mouse", result.Name);
        Assert.Equal("Ergonomic RGB mouse", result.Description);
        Assert.Equal(49.99m, result.Price);

        var inDb = await context.Products.FindAsync(result.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Gaming Mouse", inDb.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProduct_WhenExists()
    {
        // Arrange
        var (service, context) = CreateService(nameof(UpdateAsync_UpdatesProduct_WhenExists));
        var product = new Product { Name = "Old Name", Description = "Old Desc", Price = 100m };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var updateDto = new UpdateProductDto
        {
            Name = "  New Name  ",
            Description = "  New Desc  ",
            Price = 150m
        };

        // Act
        var result = await service.UpdateAsync(product.Id, updateDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Desc", result.Description);
        Assert.Equal(150m, result.Price);

        var inDb = await context.Products.FindAsync(product.Id);
        Assert.NotNull(inDb);
        Assert.Equal("New Name", inDb.Name);
        Assert.Equal(150m, inDb.Price);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        // Arrange
        var (service, _) = CreateService(nameof(UpdateAsync_ReturnsNull_WhenProductDoesNotExist));
        var updateDto = new UpdateProductDto { Name = "Name", Price = 50m };

        // Act
        var result = await service.UpdateAsync(9999, updateDto, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct_WhenExists()
    {
        // Arrange
        var (service, context) = CreateService(nameof(DeleteAsync_RemovesProduct_WhenExists));
        var product = new Product { Name = "To Delete", Price = 25m };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var success = await service.DeleteAsync(product.Id, CancellationToken.None);

        // Assert
        Assert.True(success);
        var inDb = await context.Products.FindAsync(product.Id);
        Assert.Null(inDb);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        // Arrange
        var (service, _) = CreateService(nameof(DeleteAsync_ReturnsFalse_WhenProductDoesNotExist));

        // Act
        var success = await service.DeleteAsync(9999, CancellationToken.None);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task SearchAsync_FiltersByName_AndPriceRange()
    {
        // Arrange
        var (service, context) = CreateService(nameof(SearchAsync_FiltersByName_AndPriceRange));
        context.Products.AddRange(
            new Product { Name = "Apple iPhone 15", Price = 999m },
            new Product { Name = "Apple iPad Air", Price = 599m },
            new Product { Name = "Samsung Galaxy S24", Price = 899m },
            new Product { Name = "Apple AirPods", Price = 199m }
        );
        await context.SaveChangesAsync();

        var searchDto = new ProductSearchDto
        {
            Name = "apple",
            MinPrice = 200m,
            MaxPrice = 700m
        };

        // Act
        var result = (await service.SearchAsync(searchDto, CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Apple iPad Air", result[0].Name);
    }

    [Fact]
    public async Task SearchAsync_ReturnsAll_WhenNoFilterSpecified()
    {
        // Arrange
        var (service, context) = CreateService(nameof(SearchAsync_ReturnsAll_WhenNoFilterSpecified));
        context.Products.AddRange(
            new Product { Name = "Item A", Price = 10m },
            new Product { Name = "Item B", Price = 20m }
        );
        await context.SaveChangesAsync();

        var searchDto = new ProductSearchDto();

        // Act
        var result = (await service.SearchAsync(searchDto, CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }
}
