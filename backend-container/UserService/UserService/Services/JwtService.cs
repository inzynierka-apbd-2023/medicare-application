using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserService.DTOs;

namespace UserService.Services;

public interface IJwtService
{
    string GenerateToken(string userId, string username, string role);
    string GenerateToken(UserResponseDto user);
    TokenResponseDto GenerateTokenResponse(UserResponseDto user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string userId, string username, string role)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "UserService";
        var audience = jwtSettings["Audience"] ?? "MedicareApp";
        var expiryHours = int.Parse(jwtSettings["ExpiryInHours"] ?? "24");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("userId", userId),
            new Claim("username", username),
            new Claim("role", role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(UserResponseDto user)
    {
        return GenerateToken(user.Id, user.Username, user.Role);
    }

    public TokenResponseDto GenerateTokenResponse(UserResponseDto user)
    {
        var token = GenerateToken(user);
        var expiryHours = int.Parse(_configuration.GetSection("Jwt")["ExpiryInHours"] ?? "24");

        return new TokenResponseDto
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
            User = user
        };
    }
}
