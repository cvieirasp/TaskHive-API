namespace TaskHive.Domain.Services;

public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies if a password matches a hash
    /// </summary>
    /// <param name="password">The password to verify</param>
    /// <param name="hash">The hash to verify against</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);
} 