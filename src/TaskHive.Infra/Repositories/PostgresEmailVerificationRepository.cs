using Npgsql;
using System.Data;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;
using TaskHive.Infra.Mappings;

namespace TaskHive.Infrastructure.Repositories;

public class PostgresEmailVerificationRepository(NpgsqlConnection connection) : BaseRepository(connection), IEmailVerificationRepository
{
    public async Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT id, user_id, token, expires_at, is_used, used_at, created_at
            FROM email_verification_tokens
            WHERE token = @Token";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Token", token);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return EmailVerificationMapping.MapFromReader(reader);
    }

    public async Task<EmailVerificationToken?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT id, user_id, token, expires_at, is_used, used_at, created_at
            FROM email_verification_tokens
            WHERE user_id = @UserId
            ORDER BY created_at DESC
            LIMIT 1";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("UserId", userId);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return EmailVerificationMapping.MapFromReader(reader);
    }

    public async Task<EmailVerificationToken> AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async transaction =>
        {
            const string sql = @"
            INSERT INTO email_verification_tokens (id, user_id, token, expires_at, is_used, used_at, created_at)
            VALUES (@Id, @UserId, @Token, @ExpiresAt, @IsUsed, @UsedAt, @CreatedAt)
            RETURNING id, user_id, token, expires_at, is_used, used_at, created_at";

            using var command = new NpgsqlCommand(sql, connection, transaction);
            EmailVerificationMapping.AddParameters(command, token);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new DataException("Failed to insert email verification token.");

            return EmailVerificationMapping.MapFromReader(reader);
        }, cancellationToken);
    }

    public async Task<EmailVerificationToken> UpdateAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async transaction =>
        {
            const string sql = @"
            UPDATE email_verification_tokens
            SET is_used = @IsUsed,
                used_at = @UsedAt
            WHERE id = @Id
            RETURNING id, user_id, token, expires_at, is_used, used_at, created_at";

            using var command = new NpgsqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("Id", token.Id);
            command.Parameters.AddWithValue("IsUsed", token.IsUsed);
            command.Parameters.AddWithValue("UsedAt", (object?)token.UsedAt ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new DataException("Failed to update email verification token.");

            return EmailVerificationMapping.MapFromReader(reader);
        }, cancellationToken);
    }
} 