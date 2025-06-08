using Resend;
using TaskHive.Domain.Services;

namespace TaskHive.Infrastructure.Services;

public class ResendEmailService(IResend resend, string fromEmail, string baseUrl) : IEmailService
{
    public async Task SendVerificationEmailAsync(string to, string verificationLink, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            From = fromEmail,
            To = { to },
            Subject = "Verify your email address",
            HtmlBody = $@"
                <h1>Welcome to TaskHive!</h1>
                <p>Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't create an account, you can safely ignore this email.</p>"
        };

        await resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetLink, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            From = fromEmail,
            To = { to },
            Subject = "Reset your password",
            HtmlBody = $@"
                <h1>Password Reset Request</h1>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request a password reset, you can safely ignore this email.</p>"
        };

        await resend.EmailSendAsync(message, cancellationToken);
    }
} 