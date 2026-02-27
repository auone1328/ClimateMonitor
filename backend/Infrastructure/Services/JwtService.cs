using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly JwtOptions _options;
    private readonly ITokenRepository _tokenRepository;

    public JwtService(
        IConfiguration configuration,
        UserManager<User> userManager,
        IOptions<JwtOptions> options,
        ITokenRepository tokenRepository)
    {
        _configuration = configuration;
        _userManager = userManager;
        _options = options.Value;
        _tokenRepository = tokenRepository;
    }

    public async Task<(string AccessToken, string RefreshToken)> GenerateJwtTokensAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),    
            new Claim("Email", user.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        // Добавляем роли пользователя
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey ?? throw new InvalidOperationException("JWT Key not found")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(
            double.Parse(_options.AccessTokenLifetimeMinutes ?? "60"));

        var accessToken = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: creds);
        var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

        var refreshTokenValue = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_options.RefreshTokenLifetimeDays ?? "7")),
        };

        // Ротация
        await _tokenRepository.CleanOldTokensAsync(user);

        await _tokenRepository.AddTokenAsync(refreshTokenEntity);

        return (accessTokenString, refreshTokenValue);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            ValidateLifetime = true
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken 
            || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}