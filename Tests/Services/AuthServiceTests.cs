using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EasyOrderCs.Data;
using EasyOrderCs.Services;
using EasyOrderCs.Services.Interfaces;
using EasyOrderCs.Dtos.Auth;
using EasyOrderCs.Models;
using EasyOrderCs.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Moq;
using BCrypt.Net;

namespace EasyOrderCs.Tests.Services;

public class AuthServiceTests
{
    private ApplicationDbContext CreateContext()
    {
        return TestHelpers.CreateInMemoryDbContext();
    }

    private IConfiguration CreateConfiguration()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(x => x["JWT_SECRET"]).Returns("test-secret-key-for-jwt-token-generation");
        config.Setup(x => x["JWT_EXPIRES_IN"]).Returns("24h");
        return config.Object;
    }

    private IAuthService CreateService(ApplicationDbContext context = null, IConfiguration configuration = null)
    {
        context ??= CreateContext();
        configuration ??= CreateConfiguration();
        return new AuthService(context, configuration);
    }

    [Fact]
    public async Task RegisterAsync_ComDadosValidos_DeveCriarUsuario()
    {
        // Arrange
        var service = CreateService();
        var dto = new RegisterDto
        {
            Name = "Usuário Teste",
            Email = "usuario@example.com",
            Password = "senha123"
        };

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("usuario@example.com");
        result.User.Name.Should().Be("Usuário Teste");
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_ComEmailDuplicado_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var existingUser = TestHelpers.CreateTestUser("usuario@example.com");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new RegisterDto
        {
            Name = "Outro Usuário",
            Email = "usuario@example.com", // Email duplicado
            Password = "senha123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(dto));
        exception.Message.Should().Contain("Email");
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var context = CreateContext();
        var user = TestHelpers.CreateTestUser("usuario@example.com");
        user.Password = BCrypt.Net.BCrypt.HashPassword("senha123");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new LoginDto
        {
            Email = "usuario@example.com",
            Password = "senha123"
        };

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("usuario@example.com");
    }

    [Fact]
    public async Task LoginAsync_ComEmailInexistente_DeveLancarExcecao()
    {
        // Arrange
        var service = CreateService();
        var dto = new LoginDto
        {
            Email = "inexistente@example.com",
            Password = "senha123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ComSenhaIncorreta_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var user = TestHelpers.CreateTestUser("usuario@example.com");
        user.Password = BCrypt.Net.BCrypt.HashPassword("senha123");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new LoginDto
        {
            Email = "usuario@example.com",
            Password = "senhaErrada"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ComUsuarioInativo_DeveLancarExcecao()
    {
        // Arrange
        var context = CreateContext();
        var user = TestHelpers.CreateTestUser("usuario@example.com");
        user.Password = BCrypt.Net.BCrypt.HashPassword("senha123");
        user.IsActive = false;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var dto = new LoginDto
        {
            Email = "usuario@example.com",
            Password = "senha123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(dto));
        exception.Message.Should().Contain("inativo");
    }

    [Fact]
    public async Task ValidateUserAsync_ComUsuarioAtivo_DeveRetornarUsuario()
    {
        // Arrange
        var context = CreateContext();
        var user = TestHelpers.CreateTestUser();
        user.IsActive = true;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.ValidateUserAsync(user.Id.ToString());

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUserAsync_ComUsuarioInativo_DeveRetornarNull()
    {
        // Arrange
        var context = CreateContext();
        var user = TestHelpers.CreateTestUser();
        user.IsActive = false;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.ValidateUserAsync(user.Id.ToString());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeactivateUserAsync_DeveDesativarUsuario()
    {
        // Arrange
        var context = CreateContext();
        var user = TestHelpers.CreateTestUser();
        user.IsActive = true;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        await service.DeactivateUserAsync(user.Id);

        // Assert
        var updated = await context.Users.FindAsync(user.Id);
        updated!.IsActive.Should().BeFalse();
    }
}

