using Npgsql;
using System.Data;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Enums;
using TaskHive.Domain.Extensions;

namespace TaskHive.Infrastructure.Mappings;

public static class ProjectMapping
{
    public static Project MapFromReader(IDataReader reader)
    {
        return Project.Load(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            ownerId: reader.GetGuid(reader.GetOrdinal("owner_id")),
            title: reader.GetString(reader.GetOrdinal("title")),
            description: reader.GetString(reader.GetOrdinal("description")),
            startDate: reader.GetDateTime(reader.GetOrdinal("start_date")),
            endDate: reader.GetDateTime(reader.GetOrdinal("end_date")),
            status: reader.GetString(reader.GetOrdinal("project_status")).ToEnum<ProjectStatus>(),
            projectType: reader.GetString(reader.GetOrdinal("project_type")).ToEnum<ProjectType>(),
            completedAt: reader.IsDBNull(reader.GetOrdinal("completed_at")) ? null : reader.GetDateTime(reader.GetOrdinal("completed_at")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at"))
        );
    }

    public static void AddParameters(IDbCommand command, Project project)
    {
        command.Parameters.Add(new NpgsqlParameter("Id", project.Id));
        command.Parameters.Add(new NpgsqlParameter("OwnerId", project.OwnerId));
        command.Parameters.Add(new NpgsqlParameter("Title", project.Title));
        command.Parameters.Add(new NpgsqlParameter("Description", project.Description));
        command.Parameters.Add(new NpgsqlParameter("StartDate", project.StartDate));
        command.Parameters.Add(new NpgsqlParameter("EndDate", project.EndDate));
        command.Parameters.Add(new NpgsqlParameter("ProjectStatus", project.ProjectStatus.ToString()));
        command.Parameters.Add(new NpgsqlParameter("ProjectType", project.ProjectType.ToString()));
        command.Parameters.Add(new NpgsqlParameter("CompletedAt", (object?)project.CompletedAt ?? DBNull.Value));
        command.Parameters.Add(new NpgsqlParameter("CreatedAt", project.CreatedAt));
        command.Parameters.Add(new NpgsqlParameter("UpdatedAt", project.UpdatedAt));
    }
} 