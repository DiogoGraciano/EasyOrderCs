using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EasyOrderCs.Data;
using EasyOrderCs.Services;
using EasyOrderCs.Services.Interfaces;
using EasyOrderCs.Dtos.Customer;
using EasyOrderCs.Models;
using EasyOrderCs.Tests.Helpers;
using Moq;

namespace EasyOrderCs.Tests.Services;

public class CustomerServiceTests
{
    private ApplicationDbContext CreateContext()
    {
        return TestHelpers.CreateInMemoryDbContext();
    }

    private ICustomerService CreateService(ApplicationDbContext context = null, IFileUploadService fileUploadService = null)
    {
        context ??= CreateContext();
        fileUploadService ??= Mock.Of<IFileUploadService>();
        return new CustomerService(context, fileUploadService);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCriarCliente()
    {
        // Arrange
        var context = CreateContext();
        var service = CreateService(context);
        var dto = new CreateCustomerDto
        {
            Name = "João Silva",
            Email = "joao@example.com",
            Phone = "11987654321",
            Cpf = "12345678901",
            Address = "Rua das Flores, 123"
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("João Silva");
        result.Email.Should().Be("joao@example.com");
        result.Cpf.Should().Be("12345678901");
        
        var saved = await context.Customers.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ComCPFInvalido_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateCustomerDto
        {
            Name = "João Silva",
            Email = "joao@example.com",
            Phone = "11987654321",
            Cpf = "12345678900", // CPF inválido
            Address = "Rua das Flores, 123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComEmailDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var existingCustomer = TestHelpers.CreateTestCustomer("joao@example.com");
        context.Customers.Add(existingCustomer);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateCustomerDto
        {
            Name = "João Silva",
            Email = "joao@example.com",
            Phone = "11987654321",
            Cpf = "98765432100",
            Address = "Rua das Flores, 123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("email");
    }

    [Fact]
    public async Task CreateAsync_ComCPFDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var existingCustomer = TestHelpers.CreateTestCustomer("joao@example.com", "12345678901");
        context.Customers.Add(existingCustomer);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new CreateCustomerDto
        {
            Name = "Maria Silva",
            Email = "maria@example.com",
            Phone = "11987654321",
            Cpf = "12345678901",
            Address = "Rua das Flores, 123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
        exception.Message.Should().Contain("CPF");
    }

    [Fact]
    public async Task CreateAsync_ComNomeMuitoCurto_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateCustomerDto
        {
            Name = "A", // Muito curto
            Email = "joao@example.com",
            Phone = "11987654321",
            Cpf = "12345678901",
            Address = "Rua das Flores, 123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComNomeComCaracteresInvalidos_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateCustomerDto
        {
            Name = "João123", // Contém números
            Email = "joao@example.com",
            Phone = "11987654321",
            Cpf = "12345678901",
            Address = "Rua das Flores, 123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComEnderecoMuitoCurto_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateCustomerDto
        {
            Name = "João Silva",
            Email = "joao@example.com",
            Phone = "11987654321",
            Cpf = "12345678901",
            Address = "Rua" // Muito curto
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ComTelefoneInvalido_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateCustomerDto
        {
            Name = "João Silva",
            Email = "joao@example.com",
            Phone = "123", // Inválido
            Cpf = "12345678901",
            Address = "Rua das Flores, 123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task GetByIdAsync_ComIdValido_DeveRetornarCliente()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ComIdInexistente_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var id = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(id));
    }

    [Fact]
    public async Task UpdateAsync_ComDadosValidos_DeveAtualizarCliente()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new UpdateCustomerDto
        {
            Name = "João Silva Atualizado",
            Phone = "11876543210"
        };

        // Act
        var result = await service.UpdateAsync(customer.Id, dto);

        // Assert
        result.Name.Should().Be("João Silva Atualizado");
        result.Phone.Should().Be("11876543210");
    }

    [Fact]
    public async Task UpdateAsync_ComEmailDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var customer1 = TestHelpers.CreateTestCustomer("cliente1@example.com");
        var customer2 = TestHelpers.CreateTestCustomer("cliente2@example.com");
        context.Customers.AddRange(customer1, customer2);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new UpdateCustomerDto
        {
            Email = "cliente2@example.com" // Email já usado por outro cliente
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(customer1.Id, dto));
        exception.Message.Should().Contain("email");
    }

    [Fact]
    public async Task DeleteAsync_ComClienteSemPedidos_DeveDeletarCliente()
    {
        // Arrange
        var context = CreateContext();
        var customer = TestHelpers.CreateTestCustomer();
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        await service.DeleteAsync(customer.Id);

        // Assert
        var deleted = await context.Customers.FindAsync(customer.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ComClienteComPedidos_DeveLancarExcecao()
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
            TotalAmount = 100.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(customer.Id));
        exception.Message.Should().Contain("pedidos");
    }
}

