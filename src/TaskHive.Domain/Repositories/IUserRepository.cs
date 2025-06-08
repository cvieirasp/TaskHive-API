using TaskHive.Domain.Common;
using TaskHive.Domain.Entities;

namespace TaskHive.Domain.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their ID
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their Email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists with the given email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user
    /// </summary>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza a verificação de e-mail para o usuário
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpdateEmailVerificationAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    Task<PaginatedResult<User>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
} 