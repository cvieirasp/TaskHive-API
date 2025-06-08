using TaskHive.Domain.Entities;

namespace TaskHive.Domain.Repositories;

public interface IEmailVerificationRepository
{
    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken> AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken> UpdateAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
} 