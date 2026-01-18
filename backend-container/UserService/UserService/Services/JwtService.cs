using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserService.DTOs;

namespace UserService.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username, string role);
    string GenerateToken(UserResponseDto user);
    TokenResponseDto GenerateTokenResponse(UserResponseDto user);
    (string token, DateTime expiresAt) GenerateAccessToken(UserResponseDto user);
    (string refreshToken, DateTime expiresAt, string hash) GenerateRefreshToken();
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string username, string role)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "UserService";
        var audience = jwtSettings["Audience"] ?? "MedicareApp";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "15");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim("role", role), 
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
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
        var (token, expiresAt) = GenerateAccessToken(user);
        return new TokenResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = user
        };
    }

    public (string token, DateTime expiresAt) GenerateAccessToken(UserResponseDto user)
    {
        var token = GenerateToken(user);
        var expiryMinutes = int.Parse(_configuration.GetSection("Jwt")["ExpiryInMinutes"] ?? "15");
        return (token, DateTime.UtcNow.AddMinutes(expiryMinutes));
    }

    public (string refreshToken, DateTime expiresAt, string hash) GenerateRefreshToken()
    {
        var refreshSettings = _configuration.GetSection("RefreshToken");
        var days = int.Parse(refreshSettings["ExpiryInDays"] ?? "7");
        var secureBytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(secureBytes);
        var token = Convert.ToBase64String(secureBytes);
        var hash = ComputeSha256(token);
        return (token, DateTime.UtcNow.AddDays(days), hash);
    }

    private static string ComputeSha256(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes); // 44 chars
    }
}
