using FluentAssertions;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Exceptions;

namespace TaskHive.UnitTests.Entities;

public class UserRegistrationTests
{
    [Fact]
    public void CreateUser_WithValidEmailPassword_ShouldSucceed()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed_password_123";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.CreateWithEmailAndPassword(email, passwordHash, firstName, lastName);

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsEmailVerified.Should().BeFalse();
        user.TwoFactorEnabled.Should().BeFalse();
        user.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    public void CreateUser_WithInvalidEmail_ShouldThrowDomainException(string invalidEmail)
    {
        // Arrange
        var passwordHash = "hashed_password_123";
        var firstName = "John";
        var lastName = "Doe";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            User.CreateWithEmailAndPassword(invalidEmail, passwordHash, firstName, lastName)
        );
        
        exception.Message.Should().Be("Invalid email format.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateUser_WithInvalidPasswordHash_ShouldThrowDomainException(string invalidPasswordHash)
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            User.CreateWithEmailAndPassword(email, invalidPasswordHash, firstName, lastName)
        );

        exception.Message.Should().Be("Password cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateUser_WithInvalidFirstName_ShouldThrowDomainException(string invalidFirstName)
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed_password_123";
        var lastName = "Doe";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            User.CreateWithEmailAndPassword(email, passwordHash, invalidFirstName, lastName)
        );

        exception.Message.Should().Be("FirstName cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateUser_WithInvalidLastName_ShouldThrowDomainException(string invalidLastName)
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed_password_123";
        var firstName = "John";

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            User.CreateWithEmailAndPassword(email, passwordHash, firstName, invalidLastName)
        );

        exception.Message.Should().Be("LastName cannot be empty.");
    }

    [Fact]
    public void CreateUser_WithGoogleOAuth_ShouldSetOAuthProvider()
    {
        // Arrange
        var email = "test@gmail.com";
        var firstName = "John";
        var lastName = "Doe";
        var googleId = "google_123";

        // Act
        var user = User.CreateWithOAuth(
            email: email,
            firstName: firstName,
            lastName: lastName,
            oauthProvider: "Google",
            oauthId: googleId
        );

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
        user.OAuthProvider.Should().Be("Google");
        user.OAuthId.Should().Be(googleId);
        user.PasswordHash.Should().BeNull();
        user.IsEmailVerified.Should().BeTrue(); // Google accounts are pre-verified
    }

    [Fact]
    public void EnableTwoFactor_ShouldUpdateTwoFactorStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = User.Load(
            id: id,
            email: "test@example.com",
            passwordHash: "hashed_password_123",
            firstName: "John",
            lastName: "Doe",
            isEmailVerified: true,
            twoFactorEnabled: false,
            isActive: true,
            oauthProvider: null,
            oauthId: null,
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow
        );

        // Act
        user.EnableTwoFactor();

        // Assert
        user.TwoFactorEnabled.Should().BeTrue();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void VerifyEmail_ShouldUpdateEmailVerificationStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = User.Load(
            id: id,
            email: "test@example.com",
            passwordHash: "hashed_password_123",
            firstName: "John",
            lastName: "Doe",
            isEmailVerified: false,
            twoFactorEnabled: false,
            isActive: true,
            oauthProvider: null,
            oauthId: null,
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow
        );

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailVerified.Should().BeTrue();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}