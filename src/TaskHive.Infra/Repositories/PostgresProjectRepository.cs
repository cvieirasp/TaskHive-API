using System.Data;
using Npgsql;
using TaskHive.Domain.Common;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Enums;
using TaskHive.Domain.Repositories;
using TaskHive.Infrastructure.Mappings;

namespace TaskHive.Infrastructure.Repositories;

public class PostgresProjectRepository(NpgsqlConnection connection) : BaseRepository(connection), IProjectRepository
{
    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT id, owner_id, title, description, start_date, end_date, 
                   project_status, project_type, completed_at, created_at, updated_at
            FROM projects
            WHERE id = @Id";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return ProjectMapping.MapFromReader(reader);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = "SELECT EXISTS(SELECT 1 FROM projects WHERE id = @Id)";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", id);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (bool)result!;
    }

    public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        return await ExecuteInTransactionAsync(async transaction =>
        {
            const string sql = @"
                INSERT INTO projects (id, owner_id, title, description, start_date, end_date,
                                      project_status, project_type, completed_at, created_at, updated_at)
                VALUES (@Id, @OwnerId, @Title, @Description, @StartDate, @EndDate,  
                        @ProjectStatus, @ProjectType, @CompletedAt, @CreatedAt, @UpdatedAt)
                RETURNING id, owner_id, title, description, start_date, end_date, 
                          project_status, project_type, completed_at, created_at, updated_at";

            using var command = new NpgsqlCommand(sql, connection, transaction);
            ProjectMapping.AddParameters(command, project);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new DataException("Failed to insert project.");

            return ProjectMapping.MapFromReader(reader);
        }, cancellationToken);
    }

    public async Task<PaginatedResult<Project>> GetByOwnerIdAsync(
        Guid ownerId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string countSql = "SELECT COUNT(*) FROM projects WHERE owner_id = @OwnerId";
        const string dataSql = @"
            SELECT id, owner_id, title, description, start_date, end_date, 
                   project_status, project_type, completed_at, created_at, updated_at
            FROM projects
            WHERE owner_id = @OwnerId
            ORDER BY created_at DESC
            LIMIT @PageSize
            OFFSET @Offset";

        // Get total count
        using var countCommand = new NpgsqlCommand(countSql, connection);
        countCommand.Parameters.AddWithValue("OwnerId", ownerId);
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

        // Get paginated data
        using var dataCommand = new NpgsqlCommand(dataSql, connection);
        dataCommand.Parameters.AddWithValue("OwnerId", ownerId);
        dataCommand.Parameters.AddWithValue("PageSize", pageSize);
        dataCommand.Parameters.AddWithValue("Offset", (pageNumber - 1) * pageSize);

        using var reader = await dataCommand.ExecuteReaderAsync(cancellationToken);
        var projects = new List<Project>();
        while (await reader.ReadAsync(cancellationToken))
        {
            projects.Add(ProjectMapping.MapFromReader(reader));
        }

        return new PaginatedResult<Project>(
            projects,
            pageNumber,
            pageSize,
            totalCount);
    }
} 