using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskHive.Domain.Entities;

namespace TaskHive.Infra.Mappings
{
    public static class EmailVerificationMapping
    {
        public static EmailVerificationToken MapFromReader(IDataReader reader)
        {
            return EmailVerificationToken.Load(
                id: reader.GetGuid(reader.GetOrdinal("id")),
                userId: reader.GetGuid(reader.GetOrdinal("user_id")),
                token: reader.GetString(reader.GetOrdinal("token")),
                expiresAt: reader.GetDateTime(reader.GetOrdinal("expires_at")),
                isUsed: reader.GetBoolean(reader.GetOrdinal("is_used")),
                usedAt: reader.IsDBNull(reader.GetOrdinal("used_at")) ? null : reader.GetDateTime(reader.GetOrdinal("used_at")),
                createdAt: reader.GetDateTime(reader.GetOrdinal("created_at"))
            );
        }

        public static void AddParameters(IDbCommand command, EmailVerificationToken token)
        {
            command.Parameters.Add(new NpgsqlParameter("Id", token.Id));
            command.Parameters.Add(new NpgsqlParameter("UserId", token.UserId));
            command.Parameters.Add(new NpgsqlParameter("Token", token.Token));
            command.Parameters.Add(new NpgsqlParameter("ExpiresAt", token.ExpiresAt));
            command.Parameters.Add(new NpgsqlParameter("IsUsed", token.IsUsed));
            command.Parameters.Add(new NpgsqlParameter("CreatedAt", token.CreatedAt));
            command.Parameters.Add(new NpgsqlParameter("UsedAt", (object?)token.UsedAt ?? DBNull.Value));
        }
    }
}
