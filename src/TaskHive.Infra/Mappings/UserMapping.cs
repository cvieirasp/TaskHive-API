using Npgsql;
using System.Data;
using TaskHive.Domain.Entities;

namespace TaskHive.Infrastructure.Mappings;

public static class UserMapping
{
    public static User MapFromReader(IDataReader reader)
    {
        return User.Load(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            email: reader.GetString(reader.GetOrdinal("email")),
            passwordHash: reader.IsDBNull(reader.GetOrdinal("password_hash")) ? null : reader.GetString(reader.GetOrdinal("password_hash")),
            firstName: reader.GetString(reader.GetOrdinal("first_name")),
            lastName: reader.GetString(reader.GetOrdinal("last_name")),
            isEmailVerified: reader.GetBoolean(reader.GetOrdinal("is_email_verified")),
            twoFactorEnabled: reader.GetBoolean(reader.GetOrdinal("two_factor_enabled")),
            isActive: reader.GetBoolean(reader.GetOrdinal("is_active")),
            oauthProvider: reader.IsDBNull(reader.GetOrdinal("oauth_provider")) ? null : reader.GetString(reader.GetOrdinal("oauth_provider")),
            oauthId: reader.IsDBNull(reader.GetOrdinal("oauth_id")) ? null : reader.GetString(reader.GetOrdinal("oauth_id")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at"))
        );
    }

    public static void AddParameters(IDbCommand command, User user)
    {
        command.Parameters.Add(new NpgsqlParameter("Id", user.Id));
        command.Parameters.Add(new NpgsqlParameter("Email", user.Email));
        command.Parameters.Add(new NpgsqlParameter("PasswordHash", (object?)user.PasswordHash ?? DBNull.Value));
        command.Parameters.Add(new NpgsqlParameter("FirstName", user.FirstName));
        command.Parameters.Add(new NpgsqlParameter("LastName", user.LastName));
        command.Parameters.Add(new NpgsqlParameter("IsEmailVerified", user.IsEmailVerified));
        command.Parameters.Add(new NpgsqlParameter("TwoFactorEnabled", user.TwoFactorEnabled));
        command.Parameters.Add(new NpgsqlParameter("IsActive", user.IsActive));
        command.Parameters.Add(new NpgsqlParameter("OAuthProvider", (object?)user.OAuthProvider ?? DBNull.Value));
        command.Parameters.Add(new NpgsqlParameter("OAuthId", (object?)user.OAuthId ?? DBNull.Value));
        command.Parameters.Add(new NpgsqlParameter("CreatedAt", user.CreatedAt));
        command.Parameters.Add(new NpgsqlParameter("UpdatedAt", user.UpdatedAt));
    }
} 