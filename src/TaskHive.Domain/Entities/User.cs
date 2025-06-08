using TaskHive.Domain.Exceptions;
using TaskHive.Domain.Validations;

namespace TaskHive.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public bool IsActive { get; private set; }
    public string? OAuthProvider { get; private set; }
    public string? OAuthId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    
    // Private constructor to force use of factory method
    private User(Guid id,
        string email,
        string? passwordHash,
        string firstName,
        string lastName,
        bool isEmailVerified,
        bool twoFactorEnabled,
        bool isActive,
        string? oauthProvider,
        string? oauthId,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        ValidateEmail(email);
        ValidatePasswordHash(passwordHash, oauthProvider);
        ValidateName(firstName, "FirstName");
        ValidateName(lastName, "LastName");

        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsEmailVerified = isEmailVerified;
        TwoFactorEnabled = twoFactorEnabled;
        IsActive = isActive;
        OAuthProvider = oauthProvider;
        OAuthId = oauthId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // Factory method for creation User Email/Password
    public static User CreateWithEmailAndPassword(
        string email,
        string passwordHash,
        string firstName,
        string lastName
    )
    {
        return new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            firstName,
            lastName,
            false,
            false,
            true,
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
    }

    // Factory method for creation User OAuth
    public static User CreateWithOAuth(
        string email,
        string firstName,
        string lastName,
        string oauthProvider,
        string oauthId
    )
    {
        return new User(
            Guid.NewGuid(),
            email,
            null,
            firstName,
            lastName,
            true,
            false,
            true,
            oauthProvider,
            oauthId,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
    }

    // Factory method for reconstruction
    public static User Load(
        Guid id,
        string email,
        string? passwordHash,
        string firstName,
        string lastName,
        bool isEmailVerified,
        bool twoFactorEnabled,
        bool isActive,
        string? oauthProvider,
        string? oauthId,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        return new User(
            id,
            email,
            passwordHash,
            firstName,
            lastName,
            isEmailVerified,
            twoFactorEnabled,
            isActive,
            oauthProvider,
            oauthId,
            createdAt,
            updatedAt
        );
    }

    public void EnableTwoFactor()
    {
        if (TwoFactorEnabled)
            throw new DomainException("Two-factor authentication is already enabled.");

        TwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableTwoFactor()
    {
        if (!TwoFactorEnabled)
            throw new DomainException("Two-factor authentication is not enabled.");

        TwoFactorEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
            throw new DomainException("Email is already verified.");

        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("User is already deactivated.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("User is already active.");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailValidation.IsValidEmail(email))
            throw new DomainException("Invalid email format.");
    }

    private static void ValidatePasswordHash(string? passwordHash, string? oauthProvider)
    {
        if (oauthProvider is null && string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password cannot be empty.");
    }

    private static void ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException($"{paramName} cannot be empty.");
    }
} 