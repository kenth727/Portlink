using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PortlinkApp.Api.Configuration;
using PortlinkApp.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PortlinkApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = "User creation failed", errors = result.Errors });
        }

        // Assign default role (Viewer)
        await _userManager.AddToRoleAsync(user, "Viewer");

        var token = await GenerateJwtToken(user);

        _logger.LogInformation("User {Email} registered successfully", user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = new[] { "Viewer" }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToArray()
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserInfo
        {
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToArray()
        });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName ?? user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record RegisterRequest(
    string Email,
    string Password,
    string? FullName);

public record LoginRequest(
    string Email,
    string Password);

public record AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}

public record UserInfo
{
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}
