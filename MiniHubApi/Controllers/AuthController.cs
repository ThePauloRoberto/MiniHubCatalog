using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniHubApi.Application.DTOs.Auth;
using MiniHubApi.Application.Services.Interfaces;
using System.Security.Claims;

namespace MiniHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(new
                {
                    Success = true,
                    Data = response,
                    Message = "Usuário registrado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no registro");
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(new
                {
                    Success = true,
                    Data = response,
                    Message = "Login realizado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no login");
                return Unauthorized(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Token inválido" });

                var userProfile = await _authService.GetUserProfileAsync(userId);
                return Ok(new
                {
                    Success = true,
                    Data = userProfile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário atual");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (request.NewPassword != request.ConfirmPassword)
                    return BadRequest(new { Success = false, Message = "As senhas não coincidem" });

                var success = await _authService.ChangePasswordAsync(
                    userId, 
                    request.CurrentPassword, 
                    request.NewPassword);

                if (!success)
                    return BadRequest(new { Success = false, Message = "Falha ao alterar senha" });

                return Ok(new { Success = true, Message = "Senha alterada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(new
                {
                    Success = true,
                    Data = users,
                    Count = users.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar usuários");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }

        [HttpPost("users/{userId}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] string role)
        {
            try
            {
                var success = await _authService.AssignRoleAsync(userId, role);
                
                if (!success)
                    return BadRequest(new { Success = false, Message = "Falha ao atribuir role" });

                return Ok(new { Success = true, Message = $"Role '{role}' atribuída com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atribuir role");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }

        [HttpDelete("users/{userId}/roles/{role}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            try
            {
                var success = await _authService.RemoveRoleAsync(userId, role);
                
                if (!success)
                    return BadRequest(new { Success = false, Message = "Falha ao remover role" });

                return Ok(new { Success = true, Message = $"Role '{role}' removida com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover role");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro interno do servidor"
                });
            }
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}