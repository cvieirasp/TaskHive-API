using TaskHive.Domain.Entities;
using TaskHive.Application.DTOs.Projects;

namespace TaskHive.Application.Mappings;

public static class ProjectMapping
{
    public static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            OwnerId = project.OwnerId,
            Title = project.Title,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ProjectStatus = project.ProjectStatus,
            ProjectType = project.ProjectType,
            CompletedAt = project.CompletedAt,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
} 