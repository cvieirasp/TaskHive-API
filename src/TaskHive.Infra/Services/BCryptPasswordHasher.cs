using TaskHive.Domain.Services;

namespace TaskHive.Infrastructure.Services;

public class BCryptPasswordHasher(int workFactor = 12) : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        if (string.IsNullOrEmpty(hash))
            throw new ArgumentNullException(nameof(hash));

        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
} 