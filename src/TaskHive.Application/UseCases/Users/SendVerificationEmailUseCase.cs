using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;
using TaskHive.Domain.Services;

namespace TaskHive.Application.UseCases.Users;

public class SendVerificationEmailUseCase(
    IUserRepository userRepository,
    IEmailVerificationRepository tokenRepository,
    IEmailService emailService)
{
    public async Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Get user
        var user = await userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("User not found.");
        if (user.IsEmailVerified)
            throw new InvalidOperationException("Email is already verified.");

        // Generate token
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var expiresAt = DateTime.UtcNow.AddHours(24);

        // Create verification token
        var verificationToken = EmailVerificationToken.Create(userId, token, expiresAt);
        await tokenRepository.AddAsync(verificationToken, cancellationToken);

        // Send verification email
        var baseUrl = "http://localhost:3000";
        var verificationLink = $"{baseUrl}/validate-email?token={token}";
        await emailService.SendVerificationEmailAsync(user.Email, verificationLink, cancellationToken);
    }
} 