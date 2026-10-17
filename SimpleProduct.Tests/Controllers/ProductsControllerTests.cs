using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleProduct.Server.Products.Controllers;
using SimpleProduct.Server.Products.Dtos;
using SimpleProduct.Server.Products.Services;
using Xunit;

namespace SimpleProduct.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_productServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithListOfProducts()
    {
        // Arrange
        var products = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.0m, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Product 2", Price = 20.0m, CreatedAt = DateTime.UtcNow }
        };
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenListIsEmpty()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductResponseDto>());

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(okResult.Value);
        Assert.Empty(returnedProducts);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenProductExists()
    {
        // Arrange
        var product = new ProductResponseDto { Id = 1, Name = "Test Product", Price = 15.0m };
        _productServiceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<ProductResponseDto>(okResult.Value);
        Assert.Equal(1, returned.Id);
        Assert.Equal("Test Product", returned.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductResponseDto?)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "New Laptop", Price = 999.99m, Description = "High-end laptop" };
        var createdDto = new ProductResponseDto { Id = 10, Name = "New Laptop", Price = 999.99m, Description = "High-end laptop" };
        _productServiceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _controller.Create(createDto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProductsController.GetById), createdResult.ActionName);
        Assert.Equal(10, createdResult.RouteValues?["id"]);
        var returned = Assert.IsType<ProductResponseDto>(createdResult.Value);
        Assert.Equal(10, returned.Id);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenProductFoundAndUpdated()
    {
        // Arrange
        var updateDto = new UpdateProductDto { Name = "Updated Laptop", Price = 899.99m };
        var updatedDto = new ProductResponseDto { Id = 5, Name = "Updated Laptop", Price = 899.99m };
        _productServiceMock.Setup(s => s.UpdateAsync(5, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _controller.Update(5, updateDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<ProductResponseDto>(okResult.Value);
        Assert.Equal("Updated Laptop", returned.Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateProductDto { Name = "Updated Laptop", Price = 899.99m };
        _productServiceMock.Setup(s => s.UpdateAsync(5, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductResponseDto?)null);

        // Act
        var result = await _controller.Update(5, updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeletedSuccessfully()
    {
        // Arrange
        _productServiceMock.Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(3, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProductNotFound()
    {
        // Arrange
        _productServiceMock.Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(3, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Search_ReturnsOk_WithResults()
    {
        // Arrange
        var searchDto = new ProductSearchDto { Name = "Phone", MinPrice = 100, MaxPrice = 500 };
        var searchResults = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "Smart Phone", Price = 300 }
        };
        _productServiceMock.Setup(s => s.SearchAsync(searchDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.Search(searchDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task Search_ReturnsBadRequest_WhenMinPriceGreaterThanMaxPrice()
    {
        // Arrange
        var searchDto = new ProductSearchDto { MinPrice = 500, MaxPrice = 100 };

        // Act
        var result = await _controller.Search(searchDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }
}
