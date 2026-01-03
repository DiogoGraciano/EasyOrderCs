using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EasyOrderCs.Data;
using EasyOrderCs.Services;
using EasyOrderCs.Services.Interfaces;
using EasyOrderCs.Dtos.Order;
using EasyOrderCs.Models;
using EasyOrderCs.Tests.Helpers;
using Moq;

namespace EasyOrderCs.Tests.Services;

public class OrderServiceTests
{
    private ApplicationDbContext CreateContext()
    {
        return TestHelpers.CreateInMemoryDbContext();
    }

    private IOrderService CreateService(ApplicationDbContext context = null, IProductService productService = null)
    {
        context ??= CreateContext();
        productService ??= Mock.Of<IProductService>();
        return new OrderService(context, productService);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCriarPedido()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product = TestHelpers.CreateTestProduct(enterprise.Id, "Produto 1", 10.00m, 100);
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        mockProductService.Setup(x => x.ReserveStockAsync(product.Id, 5))
            .Returns(Task.CompletedTask);

        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = "Produto 1",
                    Quantity = 5,
                    UnitPrice = 10.00m,
                    Subtotal = 50.00m
                }
            }
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.OrderNumber.Should().Be("ORD-001");
        result.TotalAmount.Should().Be(50.00m);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ComValorMinimoInferior_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product = TestHelpers.CreateTestProduct(enterprise.Id, "Produto 1", 2.00m, 100);
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 3.00m, // Menor que R$ 5,00
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = "Produto 1",
                    Quantity = 1,
                    UnitPrice = 2.00m,
                    Subtotal = 2.00m
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("mínimo");
    }

    [Fact]
    public async Task CreateAsync_ComDataNoFuturo_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product = TestHelpers.CreateTestProduct(enterprise.Id);
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.AddDays(1), // Data no futuro
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = "Produto 1",
                    Quantity = 5,
                    UnitPrice = 10.00m,
                    Subtotal = 50.00m
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComNumeroPedidoDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product = TestHelpers.CreateTestProduct(enterprise.Id);
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            Status = OrderStatus.Pending,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Orders.Add(existingOrder);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001", // Duplicado
            OrderDate = DateTime.UtcNow.Date,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = "Produto 1",
                    Quantity = 5,
                    UnitPrice = 10.00m,
                    Subtotal = 50.00m
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("número");
    }

    [Fact]
    public async Task CreateAsync_ComProdutosDuplicados_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product = TestHelpers.CreateTestProduct(enterprise.Id);
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 100.00m,
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = "Produto 1",
                    Quantity = 5,
                    UnitPrice = 10.00m,
                    Subtotal = 50.00m
                },
                new()
                {
                    ProductId = product.Id, // Produto duplicado
                    ProductName = "Produto 1",
                    Quantity = 5,
                    UnitPrice = 10.00m,
                    Subtotal = 50.00m
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("duplicados");
    }

    [Fact]
    public async Task CreateAsync_ComQuantidadeTotalExcedida_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product1 = TestHelpers.CreateTestProduct(enterprise.Id, "Produto 1");
        var product2 = TestHelpers.CreateTestProduct(enterprise.Id, "Produto 2");
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 1000.00m,
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product1.Id,
                    ProductName = "Produto 1",
                    Quantity = 30, // Total será 60, excedendo 50
                    UnitPrice = 10.00m,
                    Subtotal = 300.00m
                },
                new()
                {
                    ProductId = product2.Id,
                    ProductName = "Produto 2",
                    Quantity = 30,
                    UnitPrice = 10.00m,
                    Subtotal = 300.00m
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("exceder");
    }

    [Fact]
    public async Task CreateAsync_ComSubtotalIncorreto_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product = TestHelpers.CreateTestProduct(enterprise.Id);
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = "Produto 1",
                    Quantity = 5,
                    UnitPrice = 10.00m,
                    Subtotal = 60.00m // Incorreto, deveria ser 50.00m
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("Subtotal");
    }

    [Fact]
    public async Task CreateAsync_ComTotalIncorreto_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        var product = TestHelpers.CreateTestProduct(enterprise.Id);
        
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);
        var dto = new CreateOrderDto
        {
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 100.00m, // Incorreto, deveria ser 50.00m
            Items = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = "Produto 1",
                    Quantity = 5,
                    UnitPrice = 10.00m,
                    Subtotal = 50.00m
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("Total");
    }

    [Fact]
    public async Task UpdateStatusAsync_DePendingParaCompleted_DeveAtualizarStatus()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            Status = OrderStatus.Pending,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.UpdateStatusAsync(order.Id, OrderStatus.Completed);

        // Assert
        result.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task UpdateStatusAsync_DeCompletedParaPending_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            Status = OrderStatus.Completed,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStatusAsync(order.Id, OrderStatus.Pending));
        exception.Message.Should().Contain("completado");
    }

    [Fact]
    public async Task DeleteAsync_ComPedidoCompletado_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        var enterprise = TestHelpers.CreateTestEnterprise();
        context.Customers.Add(customer);
        context.Enterprises.Add(enterprise);
        await context.SaveChangesAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            OrderDate = DateTime.UtcNow.Date,
            Status = OrderStatus.Completed,
            CustomerId = customer.Id,
            EnterpriseId = enterprise.Id,
            TotalAmount = 50.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var mockProductService = new Mock<IProductService>();
        var service = CreateService(context, mockProductService.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(order.Id));
        exception.Message.Should().Contain("completado");
    }
}

