namespace TaskHive.Domain.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string to, string verificationLink, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string resetLink, CancellationToken cancellationToken = default);
} 