using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EasyOrderCs.Data;
using EasyOrderCs.Models;
using EasyOrderCs.Dtos.Auth;
using EasyOrderCs.Services.Interfaces;
using BCrypt.Net;

namespace EasyOrderCs.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar se o usuário já existe
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email já está em uso");
        }

        // Hash da senha
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, 10);

        // Criar usuário
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = registerDto.Name,
            Email = registerDto.Email,
            Password = hashedPassword,
            IsActive = true,
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Gerar token JWT
        var accessToken = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuário
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        // Verificar senha
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        // Verificar se o usuário está ativo
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Usuário inativo");
        }

        // Gerar token JWT
        var accessToken = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            User = MapToUserDto(user)
        };
    }

    public async Task<User?> ValidateUserAsync(string userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId && u.IsActive);
    }

    public async Task<User?> FindUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> UpdateUserAsync(Guid id, User updateData)
    {
        var user = await FindUserByIdAsync(id);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Usuário não encontrado");
        }

        user.Name = updateData.Name;
        user.Email = updateData.Email;
        user.Role = updateData.Role;
        user.IsActive = updateData.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeactivateUserAsync(Guid id)
    {
        var user = await FindUserByIdAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["JWT_SECRET"] ?? "your-secret-key";
        var jwtExpiresIn = _configuration["JWT_EXPIRES_IN"] ?? "24h";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            IsActive = user.IsActive,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

