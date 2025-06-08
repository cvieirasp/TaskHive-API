using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;
using TaskHive.Domain.Services;
using TaskHive.Application.Exceptions;

namespace TaskHive.Application.UseCases.Users;

public class SignUpUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
{
    public async Task<User> ExecuteAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        // Check if email is already in use
        if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new EmailAlreadyInUseException(email);

        // Hash password
        var passwordHash = passwordHasher.HashPassword(password);

        // Create user
        var user = User.CreateWithEmailAndPassword(email, passwordHash, firstName, lastName);

        // Save user
        return await userRepository.AddAsync(user, cancellationToken);
    }
} 