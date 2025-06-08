using TaskHive.Domain.Entities;

namespace TaskHive.Application.DTOs;

public class SignInResponse
{
    public User User { get; }
    public string Token { get; }

    public SignInResponse(User user, string token)
    {
        User = user;
        Token = token;
    }
} 