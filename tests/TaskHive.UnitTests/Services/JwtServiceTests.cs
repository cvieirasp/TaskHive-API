using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TaskHive.Domain.Entities;
using TaskHive.Infrastructure.Services;

namespace TaskHive.UnitTests.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtService _jwtService;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationInMinutes;

    public JwtServiceTests()
    {
        _secretKey = "your-256-bit-secret-key-here-for-testing-purposes-only";
        _issuer = "TaskHive";
        _audience = "TaskHiveUsers";
        _expirationInMinutes = 60;

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Jwt:SecretKey"]).Returns(_secretKey);
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(_issuer);
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(_audience);
        _configurationMock.Setup(x => x["Jwt:ExpirationInMinutes"]).Returns(_expirationInMinutes.ToString());

        _jwtService = new JwtService(_configurationMock.Object);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var user = User.CreateWithEmailAndPassword("test@example.com", "hashed_password", "John", "Doe");

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.True(_jwtService.ValidateToken(token));
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = User.CreateWithEmailAndPassword("test@example.com", "hashed_password", "John", "Doe");
        var token = _jwtService.GenerateToken(user);

        // Act
        var isValid = _jwtService.ValidateToken(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var user = User.CreateWithEmailAndPassword("test@example.com", "hashed_password", "John", "Doe");

        // Create a token with immediate expiration
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim("isEmailVerified", user.IsEmailVerified.ToString()),
            new Claim("twoFactorEnabled", user.TwoFactorEnabled.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(-1), // Expired token
            signingCredentials: credentials
        );

        var expiredToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Act
        var isValid = _jwtService.ValidateToken(expiredToken);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var user = User.CreateWithEmailAndPassword("test@example.com", "hashed_password", "John", "Doe");
        var token = _jwtService.GenerateToken(user);

        // Act
        var extractedUserId = _jwtService.GetUserIdFromToken(token);

        // Assert
        Assert.NotNull(extractedUserId);
        Assert.Equal(user.Id, extractedUserId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _jwtService.GetUserIdFromToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var user = User.CreateWithEmailAndPassword("test@example.com", "hashed_password", "John", "Doe");

        // Create a token with immediate expiration
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim("isEmailVerified", user.IsEmailVerified.ToString()),
            new Claim("twoFactorEnabled", user.TwoFactorEnabled.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(-1), // Expired token
            signingCredentials: credentials
        );

        var expiredToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Act
        var extractedUserId = _jwtService.GetUserIdFromToken(expiredToken);

        // Assert
        Assert.Null(extractedUserId);
    }
} 