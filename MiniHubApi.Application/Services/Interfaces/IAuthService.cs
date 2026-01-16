using MiniHubApi.Application.DTOs.Auth;

namespace MiniHubApi.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserProfileDto> GetUserProfileAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    
    Task<bool> AssignRoleAsync(string userId, string role);
    Task<bool> RemoveRoleAsync(string userId, string role);
    Task<List<UserProfileDto>> GetAllUsersAsync();
    
    Task SeedAdminUserAsync();
}