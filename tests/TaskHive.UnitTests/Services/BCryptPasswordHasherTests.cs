using TaskHive.Domain.Services;
using TaskHive.Infrastructure.Services;

namespace TaskHive.UnitTests.Services;

public class BCryptPasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public BCryptPasswordHasherTests()
    {
        _passwordHasher = new BCryptPasswordHasher(workFactor: 4); // Using lower work factor for faster tests
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.StartsWith("$2", hash); // BCrypt hashes start with $2
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // Due to salt, hashes should be different
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void HashPassword_WithInvalidPassword_ShouldThrowException(string? password)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _passwordHasher.HashPassword(password!));
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("TestPassword123!", "")]
    [InlineData("TestPassword123!", null)]
    [InlineData("", "validhash")]
    [InlineData(null, "validhash")]
    public void VerifyPassword_WithInvalidInput_ShouldThrowArgumentNullException(string? password, string? hash)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _passwordHasher.VerifyPassword(password!, hash!));
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldThrowArgumentNullSaltParseException()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "invalidhash";

        // Act & Assert
        Assert.Throws<BCrypt.Net.SaltParseException>(() => _passwordHasher.VerifyPassword(password, invalidHash));
    }

    [Fact]
    public void HashPassword_WithDifferentWorkFactors_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";
        var hasher1 = new BCryptPasswordHasher(workFactor: 4);
        var hasher2 = new BCryptPasswordHasher(workFactor: 12);

        // Act
        var hash1 = hasher1.HashPassword(password);
        var hash2 = hasher2.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_ShouldWork()
    {
        // Arrange
        var password = "Test@Password#123$!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.True(_passwordHasher.VerifyPassword(password, hash));
    }

    [Fact]
    public void HashPassword_WithLongPassword_ShouldWork()
    {
        // Arrange
        var password = new string('a', 1000); // Very long password

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.True(_passwordHasher.VerifyPassword(password, hash));
    }
} 