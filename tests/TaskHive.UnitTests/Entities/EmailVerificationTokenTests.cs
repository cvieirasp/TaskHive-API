using FluentAssertions;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Exceptions;

namespace TaskHive.UnitTests.Entities;

public class EmailVerificationTokenTests
{
    private readonly Guid _userId;
    private readonly string _token;
    private readonly DateTime _expiresAt;

    public EmailVerificationTokenTests()
    {
        _userId = Guid.NewGuid();
        _token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        _expiresAt = DateTime.UtcNow.AddHours(24);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateToken()
    {
        // Act
        var token = EmailVerificationToken.Create(_userId, _token, _expiresAt);

        // Assert
        token.Should().NotBeNull();
        token.Id.Should().NotBe(Guid.Empty);
        token.UserId.Should().Be(_userId);
        token.Token.Should().Be(_token);
        token.ExpiresAt.Should().Be(_expiresAt);
        token.IsUsed.Should().BeFalse();
        token.UsedAt.Should().BeNull();
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithExpiredDate_ShouldThrowException()
    {
        // Arrange
        var expiredDate = DateTime.UtcNow.AddHours(-1);

        // Act
        var act = () => EmailVerificationToken.Create(_userId, _token, expiredDate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Expiration date must be in the future.");
    }

    [Fact]
    public void MarkAsUsed_WhenNotUsed_ShouldMarkAsUsed()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_userId, _token, _expiresAt);

        // Act
        token.MarkAsUsed();

        // Assert
        token.IsUsed.Should().BeTrue();
        token.UsedAt.Should().NotBeNull();
        token.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsUsed_WhenAlreadyUsed_ShouldThrowException()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_userId, _token, _expiresAt);
        token.MarkAsUsed();

        // Act
        var act = () => token.MarkAsUsed();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Token has already been used.");
    }

    [Fact]
    public void IsValid_WhenNotUsedAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_userId, _token, _expiresAt);

        // Act
        var isValid = token.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenUsed_ShouldReturnFalse()
    {
        // Arrange
        var token = EmailVerificationToken.Create(_userId, _token, _expiresAt);
        token.MarkAsUsed();

        // Act
        var isValid = token.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var expiredDate = DateTime.UtcNow.AddSeconds(1);
        var token = EmailVerificationToken.Create(_userId, _token, expiredDate);
        
        // Wait for token to expire
        Thread.Sleep(2000);

        // Act
        var isValid = token.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }
} 