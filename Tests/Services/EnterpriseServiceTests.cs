using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EasyOrderCs.Data;
using EasyOrderCs.Services;
using EasyOrderCs.Services.Interfaces;
using EasyOrderCs.Dtos.Enterprise;
using EasyOrderCs.Models;
using EasyOrderCs.Tests.Helpers;
using Moq;

namespace EasyOrderCs.Tests.Services;

public class EnterpriseServiceTests
{
    private ApplicationDbContext CreateContext()
    {
        return TestHelpers.CreateInMemoryDbContext();
    }

    private IEnterpriseService CreateService(ApplicationDbContext context = null, IFileUploadService fileUploadService = null)
    {
        context ??= CreateContext();
        fileUploadService ??= Mock.Of<IFileUploadService>();
        return new EnterpriseService(context, fileUploadService);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCriarEmpresa()
    {
        // Arrange
        var context = CreateContext();
        var service = CreateService(context);
        var dto = new CreateEnterpriseDto
        {
            LegalName = "Empresa Teste LTDA",
            TradeName = "Empresa Teste",
            Cnpj = "12345678000190",
            Address = "Rua das Empresas, 456",
            FoundationDate = new DateTime(2000, 1, 1)
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.LegalName.Should().Be("Empresa Teste LTDA");
        result.Cnpj.Should().Be("12345678000190");
        
        var saved = await context.Enterprises.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ComCNPJInvalido_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateEnterpriseDto
        {
            LegalName = "Empresa Teste LTDA",
            TradeName = "Empresa Teste",
            Cnpj = "12345678000100", // CNPJ inválido
            Address = "Rua das Empresas, 456",
            FoundationDate = new DateTime(2000, 1, 1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComCNPJDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var existingEnterprise = TestHelpers.CreateTestEnterprise("12345678000190");
        context.Enterprises.Add(existingEnterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateEnterpriseDto
        {
            LegalName = "Outra Empresa LTDA",
            TradeName = "Outra Empresa",
            Cnpj = "12345678000190",
            Address = "Rua das Empresas, 789",
            FoundationDate = new DateTime(2000, 1, 1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("CNPJ");
    }

    [Fact]
    public async Task CreateAsync_ComRazaoSocialDuplicada_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var existingEnterprise = TestHelpers.CreateTestEnterprise();
        existingEnterprise.LegalName = "Empresa Teste LTDA";
        context.Enterprises.Add(existingEnterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateEnterpriseDto
        {
            LegalName = "Empresa Teste LTDA",
            TradeName = "Outra Empresa",
            Cnpj = "98765432000110",
            Address = "Rua das Empresas, 789",
            FoundationDate = new DateTime(2000, 1, 1)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("razão social");
    }

    [Fact]
    public async Task CreateAsync_ComDataFundacaoNoFuturo_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateEnterpriseDto
        {
            LegalName = "Empresa Teste LTDA",
            TradeName = "Empresa Teste",
            Cnpj = "12345678000190",
            Address = "Rua das Empresas, 456",
            FoundationDate = DateTime.UtcNow.AddDays(1) // Data no futuro
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComRazaoSocialMuitoCurta_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateEnterpriseDto
        {
            LegalName = "AB", // Muito curto
            TradeName = "Empresa Teste",
            Cnpj = "12345678000190",
            Address = "Rua das Empresas, 456",
            FoundationDate = new DateTime(2000, 1, 1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task DeleteAsync_ComEmpresaSemProdutosEPedidos_DeveDeletarEmpresa()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        await service.DeleteAsync(enterprise.Id);

        // Assert
        var deleted = await context.Enterprises.FindAsync(enterprise.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ComEmpresaComProdutos_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var product = TestHelpers.CreateTestProduct(enterprise.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(enterprise.Id));
        exception.Message.Should().Contain("produtos");
    }

    [Fact]
    public async Task DeleteAsync_ComEmpresaComPedidos_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var customer = TestHelpers.CreateTestCustomer();
        context.Enterprises.Add(enterprise);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            Status = OrderStatus.Pending,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 100.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(enterprise.Id));
        exception.Message.Should().Contain("pedidos");
    }
}

