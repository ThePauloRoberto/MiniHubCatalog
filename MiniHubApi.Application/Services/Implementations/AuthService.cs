using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniHubApi.Application.DTOs.Auth;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Domain.Entities;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MiniHubApi.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
         private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditService _auditService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    throw new Exception($"Usuário com email {request.Email} já existe.");
                }
                
                var user = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                var result = await _userManager.CreateAsync(user, request.Password);
                
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Falha ao criar usuário: {errors}");
                }
                
                await _userManager.AddToRoleAsync(user, "Viewer");
                
                await _auditService.LogActionAsync("Register", "User", user.Id, 
                    $"Novo usuário registrado: {user.Email}", user.Id);

                _logger.LogInformation("Usuário registrado: {Email} ({UserId})", user.Email, user.Id);
                
                return await GenerateAuthResponseAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário");
                throw;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    throw new Exception("Credenciais inválidas");
                }
                
                if (!user.IsActive)
                {
                    throw new Exception("Conta de usuário desativada");
                }
                
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!, 
                    request.Password, 
                    isPersistent: false, 
                    lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        throw new Exception("Conta temporariamente bloqueada devido a muitas tentativas falhas");
                    }
                    throw new Exception("Credenciais inválidas");
                }
                
                await _auditService.LogActionAsync("Login", "User", user.Id, 
                    "Login realizado com sucesso", user.Id);

                _logger.LogInformation("Usuário logado: {Email} ({UserId})", user.Email, user.Id);
                
                return await GenerateAuthResponseAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no login");
                throw;
            }
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Usuário não encontrado");

            var userDto = MapToUserProfileDto(user);
            userDto.Roles = await GetUserRolesAsync(userId);
            return userDto;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            
            if (result.Succeeded)
            {
                await _auditService.LogActionAsync("ChangePassword", "User", user.Id, 
                    "Senha alterada com sucesso", user.Id);
            }

            return result.Succeeded;
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;
            
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
                return false;

            var result = await _userManager.AddToRoleAsync(user, role);
            
            if (result.Succeeded)
            {
                await _auditService.LogActionAsync("AssignRole", "User", user.Id, 
                    $"Role '{role}' atribuída ao usuário", "SYSTEM");
            }

            return result.Succeeded;
        }

        public async Task<bool> RemoveRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            
            if (result.Succeeded)
            {
                await _auditService.LogActionAsync("RemoveRole", "User", user.Id, 
                    $"Role '{role}' removida do usuário", "SYSTEM");
            }

            return result.Succeeded;
        }

        public async Task<List<UserProfileDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userDtos = new List<UserProfileDto>();

            foreach (var user in users)
            {
                var dto = MapToUserProfileDto(user);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(dto);
            }

            return userDtos;
        }

        public async Task SeedAdminUserAsync()
        {
            try
            {
                var defaultRoles = new[] { "Admin", "Editor", "Viewer" };
                
                foreach (var roleName in defaultRoles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                        _logger.LogInformation("Role criada: {Role}", roleName);
                    }
                }
                
                var adminEmail = _configuration["AdminUser:Email"] ?? "admin@minihub.com";
                var adminPassword = _configuration["AdminUser:Password"] ?? "Admin@123";

                var adminUser = await _userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        Email = adminEmail,
                        UserName = adminEmail,
                        FirstName = "Administrador",
                        LastName = "Sistema",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        foreach (var role in defaultRoles)
                        {
                            await _userManager.AddToRoleAsync(adminUser, role);
                        }

                        await _auditService.LogActionAsync("Seed", "User", adminUser.Id, 
                            "Usuário admin criado pelo sistema", "SYSTEM");

                        _logger.LogInformation("Usuário admin criado: {Email}", adminEmail);
                    }
                    else
                    {
                        _logger.LogError("Falha ao criar admin: {Errors}", 
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário admin");
            }
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user)
        {
            var token = await GenerateJwtTokenAsync(user);
            var roles = await GetUserRolesAsync(user.Id);
            var userProfile = MapToUserProfileDto(user);
            userProfile.Roles = roles;

            return new AuthResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = userProfile,
                Roles = roles
            };
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim("fullName", user.FullName)
            };
            
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtKey = _configuration["Jwt:Key"] ?? "SuaChaveSuperSecretaMinima32Caracteres1234567890!!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "MiniHubAPI",
                audience: _configuration["Jwt:Audience"] ?? "MiniHubClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserProfileDto MapToUserProfileDto(ApplicationUser user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Roles = new List<string>()
            };
        }
    }
}