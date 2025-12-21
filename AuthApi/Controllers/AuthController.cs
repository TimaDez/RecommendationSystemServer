using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AuthDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public record SignUpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; init; }
    }
    
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token, string Email);

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        Console.WriteLine($"User input email: {request.Email}, password: {request.Password}");
        var email = request.Email.Trim().ToLowerInvariant();
        Console.WriteLine($"Normalized email: {email}");
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict("User with this email already exists.");

        var passwordHash = HashPassword(request.Password);

        var user = new AuthUser
        {
            Email = email,
            PasswordHash = passwordHash
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponse(token, user.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            return Unauthorized("Invalid email or password.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponse(token, user.Email));
    }

    // ===== Helpers =====

    private string GenerateJwtToken(AuthUser user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key");
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Missing configuration: Jwt:Key (Jwt__Key env var).");

        var issuer    = jwtSection.GetValue<string>("Issuer")  ?? "RecommendationSystem";
        var audience  = jwtSection.GetValue<string>("Audience")?? "RecommendationSystemClients";

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds      = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // super-simple hash just for demo; you can swap to bcrypt/Identity later
    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash  = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash)
        => HashPassword(password) == storedHash;
}
