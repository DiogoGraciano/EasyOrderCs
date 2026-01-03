using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EasyOrderCs.Data;
using EasyOrderCs.Services;
using EasyOrderCs.Services.Interfaces;
using EasyOrderCs.Dtos.Product;
using EasyOrderCs.Models;
using EasyOrderCs.Tests.Helpers;
using Moq;

namespace EasyOrderCs.Tests.Services;

public class ProductServiceTests
{
    private ApplicationDbContext CreateContext()
    {
        return TestHelpers.CreateInMemoryDbContext();
    }

    private IProductService CreateService(ApplicationDbContext context = null, IFileUploadService fileUploadService = null)
    {
        context ??= CreateContext();
        fileUploadService ??= Mock.Of<IFileUploadService>();
        return new ProductService(context, fileUploadService);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCriarProduto()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Descrição do produto teste com mais de 10 caracteres",
            Price = 29.99m,
            Stock = 100,
            EnterpriseId = enterprise.Id
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Produto Teste");
        result.Price.Should().Be(29.99m);
        result.Stock.Should().Be(100);
    }

    [Fact]
    public async Task CreateAsync_ComPrecoZero_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Descrição do produto teste",
            Price = 0, // Preço zero
            Stock = 100,
            EnterpriseId = enterprise.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComPrecoNegativo_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Descrição do produto teste",
            Price = -10.00m, // Preço negativo
            Stock = 100,
            EnterpriseId = enterprise.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComPrecoMuitoAlto_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Descrição do produto teste",
            Price = 2000000.00m, // Muito alto
            Stock = 100,
            EnterpriseId = enterprise.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComEstoqueNegativo_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateProductDto
        {
            Name = "Produto Teste",
            Description = "Descrição do produto teste",
            Price = 10.00m,
            Stock = -10, // Estoque negativo
            EnterpriseId = enterprise.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicadoNaMesmaEmpresa_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var existingProduct = TestHelpers.CreateTestProduct(enterprise.Id, "Produto Teste");
        context.Products.Add(existingProduct);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateProductDto
        {
            Name = "Produto Teste", // Nome duplicado
            Description = "Descrição do produto teste",
            Price = 10.00m,
            Stock = 100,
            EnterpriseId = enterprise.Id
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("nome");
    }

    [Fact]
    public async Task CreateAsync_ComNomeDuplicadoEmEmpresasDiferentes_DevePermitir()
    {
        // Arrange
        var context = CreateContext();
        var enterprise1 = TestHelpers.CreateTestEnterprise("12345678000190");
        var enterprise2 = TestHelpers.CreateTestEnterprise("98765432000110");
        context.Enterprises.AddRange(enterprise1, enterprise2);
        await context.SaveChangesAsync();

        var existingProduct = TestHelpers.CreateTestProduct(enterprise1.Id, "Produto Teste");
        context.Products.Add(existingProduct);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateProductDto
        {
            Name = "Produto Teste", // Mesmo nome, empresa diferente
            Description = "Descrição do produto teste",
            Price = 10.00m,
            Stock = 100,
            EnterpriseId = enterprise2.Id
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Produto Teste");
    }

    [Fact]
    public async Task UpdateStockAsync_ComQuantidadePositiva_DeveAumentarEstoque()
    {
        // Arrange
        var context = CreateContext();
        var product = TestHelpers.CreateTestProduct(Guid.NewGuid(), "Produto Teste", 10.00m, 100);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.UpdateStockAsync(product.Id, 50);

        // Assert
        result.Stock.Should().Be(150);
    }

    [Fact]
    public async Task UpdateStockAsync_ComQuantidadeNegativa_DeveDiminuirEstoque()
    {
        // Arrange
        var context = CreateContext();
        var product = TestHelpers.CreateTestProduct(Guid.NewGuid(), "Produto Teste", 10.00m, 100);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.UpdateStockAsync(product.Id, -30);

        // Assert
        result.Stock.Should().Be(70);
    }

    [Fact]
    public async Task UpdateStockAsync_ComEstoqueInsuficiente_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var product = TestHelpers.CreateTestProduct(Guid.NewGuid(), "Produto Teste", 10.00m, 50);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateStockAsync(product.Id, -100));
        exception.Message.Should().Contain("insuficiente");
    }

    [Fact]
    public async Task UpdateStockAsync_ComQuantidadeZero_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var product = TestHelpers.CreateTestProduct(Guid.NewGuid(), "Produto Teste", 10.00m, 100);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateStockAsync(product.Id, 0));
    }

    [Fact]
    public async Task ReserveStockAsync_ComEstoqueDisponivel_DeveReservarEstoque()
    {
        // Arrange
        var context = CreateContext();
        var product = TestHelpers.CreateTestProduct(Guid.NewGuid(), "Produto Teste", 10.00m, 100);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        await service.ReserveStockAsync(product.Id, 30);

        // Assert
        var updated = await context.Products.FindAsync(product.Id);
        updated!.Stock.Should().Be(70);
    }

    [Fact]
    public async Task ReserveStockAsync_ComEstoqueInsuficiente_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var product = TestHelpers.CreateTestProduct(Guid.NewGuid(), "Produto Teste", 10.00m, 50);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveStockAsync(product.Id, 100));
        exception.Message.Should().Contain("insuficiente");
    }

    [Fact]
    public async Task ReleaseStockAsync_DeveLiberarEstoque()
    {
        // Arrange
        var context = CreateContext();
        var product = TestHelpers.CreateTestProduct(Guid.NewGuid(), "Produto Teste", 10.00m, 50);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        await service.ReleaseStockAsync(product.Id, 30);

        // Assert
        var updated = await context.Products.FindAsync(product.Id);
        updated!.Stock.Should().Be(80);
    }
}

