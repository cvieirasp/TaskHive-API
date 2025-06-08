using TaskHive.Domain.Entities;

namespace TaskHive.Domain.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
} 