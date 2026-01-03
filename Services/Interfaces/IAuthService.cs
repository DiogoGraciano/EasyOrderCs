using EasyOrderCs.Dtos.Auth;
using EasyOrderCs.Models;

namespace EasyOrderCs.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<User?> ValidateUserAsync(string userId);
    Task<User?> FindUserByIdAsync(Guid id);
    Task<User> UpdateUserAsync(Guid id, User updateData);
    Task DeactivateUserAsync(Guid id);
}

