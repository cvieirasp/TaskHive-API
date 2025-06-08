using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;
using TaskHive.Domain.Services;
using TaskHive.Domain.Exceptions;
using TaskHive.Application.DTOs;
using TaskHive.Application.Exceptions;

namespace TaskHive.Application.UseCases.Users;

public class SignInUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public SignInUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<SignInResponse> ExecuteAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty.");

        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password cannot be empty.");

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
            throw new InvalidCredentialsException();

        // Verify password
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            throw new InvalidCredentialsException();

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return new SignInResponse(user, token);
    }
} 