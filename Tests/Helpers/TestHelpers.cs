using Microsoft.EntityFrameworkCore;
using EasyOrderCs.Data;
using EasyOrderCs.Models;
using BCrypt.Net;

namespace EasyOrderCs.Tests.Helpers;

public static class TestHelpers
{
    public static ApplicationDbContext CreateInMemoryDbContext(string dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static User CreateTestUser(string email = "test@example.com", string name = "Test User")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
            IsActive = true,
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Customer CreateTestCustomer(string email = "customer@example.com", string cpf = "12345678901")
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = email,
            Phone = "11999999999",
            Cpf = cpf,
            Address = "Rua Teste, 123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Enterprise CreateTestEnterprise(string cnpj = "12345678000190")
    {
        return new Enterprise
        {
            Id = Guid.NewGuid(),
            LegalName = "Empresa Teste LTDA",
            TradeName = "Empresa Teste",
            Cnpj = cnpj,
            Address = "Rua Teste, 456",
            FoundationDate = new DateTime(2000, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Product CreateTestProduct(Guid enterpriseId, string name = "Produto Teste", decimal price = 10.50m, int stock = 100)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Descrição do produto teste",
            Price = price,
            Stock = stock,
            EnterpriseId = enterpriseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

