using System.ComponentModel.DataAnnotations;
using Interviu.Core.DTOs;
using Interviu.Service.Exceptions;
using Interviu.Service.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Interviu.Entity.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Interviu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public UserController(
        IUserService userService, 
        ILogger<UserController> logger,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userService = userService;
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    success = false, 
                    message = "Geçersiz veri", 
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                });
            }

            var user = await _userService.RegisterUserAsync(dto);
                
            return CreatedAtAction(
                nameof(GetUserById), 
                new { id = user.Id }, 
                new 
                { 
                    success = true, 
                    message = "Kullanıcı başarıyla kaydedildi", 
                    data = user 
                }
            );
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu");
            return StatusCode(500, new { success = false, message = "Bir hata oluştu" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    success = false, 
                    message = "Geçersiz veri" 
                });
            }

            // Kullanıcıyı email ile bul
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized(new 
                { 
                    success = false, 
                    message = "E-posta veya şifre hatalı" 
                });
            }

            // Identity ile giriş yap
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, 
                dto.Password, 
                isPersistent: false,  // "Beni Hatırla" özelliği
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                return Unauthorized(new 
                { 
                    success = false, 
                    message = "E-posta veya şifre hatalı" 
                });
            }

            // Kullanıcı bilgileri
            var userDto = await _userService.GetUserByEmailAsync(dto.Email);

            // JWT üret
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["ExpiresMinutes"]!)),
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                success = true,
                message = "Giriş başarılı",
                data = new
                {
                    user = userDto,
                    access_token = tokenString,
                    expires_in = (int)TimeSpan.FromMinutes(int.Parse(jwtSection["ExpiresMinutes"]!)).TotalSeconds
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login sırasında hata oluştu");
            return StatusCode(500, new { success = false, message = "Bir hata oluştu" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            
            return Ok(new 
            { 
                success = true, 
                message = "Çıkış başarılı" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout sırasında hata oluştu");
            return StatusCode(500, new { success = false, message = "Bir hata oluştu" });
        }
    }

    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Kullanıcı bulunamadı" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            
            return Ok(new 
            { 
                success = true, 
                data = user 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı getirme sırasında hata oluştu");
            return StatusCode(500, new { success = false, message = "Bir hata oluştu" });
        }
    }
    
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
                
            return Ok(new 
            { 
                success = true, 
                data = user 
            });
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı getirme sırasında hata oluştu");
            return StatusCode(500, new { success = false, message = "Bir hata oluştu" });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();

            return Ok(new
            {
                success = true,
                data = users,
                count = users.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcıları getirme sırasında hata oluştu");
            return StatusCode(500, new { success = false, message = "Bir hata oluştu" });
        }
    }

    [HttpGet("by-email/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Kullanıcı bulunamadı"
                });
            }

            return Ok(new
            {
                success = true,
                data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı getirme sırasında bir hata olustu");
            return StatusCode(500, new { success = false, message = "Bir hata olustu" });
        }
    }

    [HttpGet("with-cv")]
    [Authorize]
    public async Task<IActionResult> GetUsersWithCvAsync()
    {
        try
        {
            var users = await _userService.GetUsersWithCvAsync();

            return Ok(new
            {
                success = true,
                data = users,
                count = users.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cv'li kullanıcıları getirme sırasında bir hata oluştu");
            return StatusCode(500, new { success = false, message = "Bir hata oluştu" });
        }
    }
}