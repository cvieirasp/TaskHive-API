using TaskHive.Domain.Repositories;

namespace TaskHive.Application.UseCases.Users;

public class VerifyEmailUseCase(
    IUserRepository userRepository,
    IEmailVerificationRepository tokenRepository)
{
    public async Task ExecuteAsync(string token, CancellationToken cancellationToken = default)
    {
        // Get verification token
        var verificationToken = await tokenRepository.GetByTokenAsync(token, cancellationToken) ?? throw new InvalidOperationException("Invalid verification token.");
        if (!verificationToken.IsValid())
            throw new InvalidOperationException("Verification token has expired or has already been used.");

        // Get user
        var user = await userRepository.GetByIdAsync(verificationToken.UserId, cancellationToken) ?? throw new InvalidOperationException("User not found.");
        if (user.IsEmailVerified)
            throw new InvalidOperationException("Email is already verified.");

        // Mark token as used
        verificationToken.MarkAsUsed();
        await tokenRepository.UpdateAsync(verificationToken, cancellationToken);

        // Verify user's email
        user.VerifyEmail();
        await userRepository.UpdateEmailVerificationAsync(user, cancellationToken);
    }
} 