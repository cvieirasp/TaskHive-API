using System;
using TaskHive.Domain.Exceptions;

namespace TaskHive.Domain.Entities;

public class EmailVerificationToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private EmailVerificationToken(
        Guid id, 
        Guid userId,
        string token,
        DateTime expiresAt,
        bool isUsed,
        DateTime? usedAt,
        DateTime createdAt
    )
    {
        if (expiresAt < DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        Id = id;
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        IsUsed = isUsed;
        UsedAt = usedAt;
        CreatedAt = createdAt;
    }

    public static EmailVerificationToken Create(Guid userId, string token, DateTime expiresAt)
    {
        return new EmailVerificationToken(
            id: Guid.NewGuid(),
            userId: userId,
            token: token,
            expiresAt: expiresAt,
            isUsed: false,
            usedAt: null,
            createdAt: DateTime.UtcNow
        );
    }

    public static EmailVerificationToken Load(
        Guid id,
        Guid userId,
        string token,
        DateTime expiresAt,
        bool isUsed,
        DateTime? usedAt,
        DateTime createdAt
    )
    {
        return new EmailVerificationToken(
            id: id,
            userId: userId,
            token: token,
            expiresAt: expiresAt,
            isUsed: isUsed,
            usedAt: usedAt,
            createdAt: createdAt
        );
    }
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new DomainException("Token has already been used.");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow <= ExpiresAt;
    }
} 