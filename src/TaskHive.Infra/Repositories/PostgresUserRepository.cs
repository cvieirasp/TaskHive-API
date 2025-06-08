using System.Data;
using Npgsql;
using TaskHive.Domain.Common;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;
using TaskHive.Infrastructure.Mappings;

namespace TaskHive.Infrastructure.Repositories;

public class PostgresUserRepository(NpgsqlConnection connection) : BaseRepository(connection), IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT id, email, password_hash, first_name, last_name, is_email_verified, 
                   two_factor_enabled, is_active, oauth_provider, oauth_id, 
                   created_at, updated_at
            FROM users
            WHERE id = @Id";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return UserMapping.MapFromReader(reader);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT id, email, password_hash, first_name, last_name, is_email_verified, 
                   two_factor_enabled, is_active, oauth_provider, oauth_id, 
                   created_at, updated_at
            FROM users
            WHERE email = @Email";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Email", email.ToLowerInvariant());

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return UserMapping.MapFromReader(reader);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = "SELECT EXISTS(SELECT 1 FROM users WHERE email = @Email)";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Email", email.ToLowerInvariant());

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)result!;
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async transaction =>
        {
            const string sql = @"
                INSERT INTO users (id, email, password_hash, first_name, last_name, 
                                   is_email_verified, two_factor_enabled, is_active, 
                                   oauth_provider, oauth_id, created_at, updated_at)
                VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, 
                        @IsEmailVerified, @TwoFactorEnabled, @IsActive, 
                        @OAuthProvider, @OAuthId, @CreatedAt, @UpdatedAt)
                RETURNING id, email, password_hash, first_name, last_name, 
                          is_email_verified, two_factor_enabled, is_active,
                          oauth_provider, oauth_id, created_at, updated_at";

            using var command = new NpgsqlCommand(sql, connection, transaction);
            UserMapping.AddParameters(command, user);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new DataException("Failed to insert user.");

            return UserMapping.MapFromReader(reader);
        }, cancellationToken);
    }

    public async Task<bool> UpdateEmailVerificationAsync(User user, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async transaction =>
        {
            const string sql = @"
                UPDATE users
                SET is_email_verified = @IsEmailVerified,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

            using var command = new NpgsqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("Id", user.Id);
            command.Parameters.AddWithValue("IsEmailVerified", user.IsEmailVerified);
            command.Parameters.AddWithValue("UpdatedAt", user.UpdatedAt);

            var updated = await command.ExecuteNonQueryAsync(cancellationToken);
            return (updated > 0);

        }, cancellationToken);
    }

    public async Task<PaginatedResult<User>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string countSql = "SELECT COUNT(*) FROM users";
        const string dataSql = @"
            SELECT id, email, password_hash, first_name, last_name, is_email_verified, 
                   two_factor_enabled, is_active, oauth_provider, oauth_id, 
                   created_at, updated_at
            FROM users
            ORDER BY created_at DESC
            LIMIT @PageSize
            OFFSET @Offset";

        // Get total count
        using var countCommand = new NpgsqlCommand(countSql, connection);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

        // Get paginated data
        using var dataCommand = new NpgsqlCommand(dataSql, connection);
        dataCommand.Parameters.AddWithValue("PageSize", pageSize);
        dataCommand.Parameters.AddWithValue("Offset", (pageNumber - 1) * pageSize);

        using var reader = await dataCommand.ExecuteReaderAsync(cancellationToken);
        var users = new List<User>();
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(UserMapping.MapFromReader(reader));
        }

        return new PaginatedResult<User>(
            users,
            pageNumber,
            pageSize,
            totalCount);
    }
} 