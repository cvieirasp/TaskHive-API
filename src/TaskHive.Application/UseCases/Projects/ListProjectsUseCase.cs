using TaskHive.Domain.Common;
using TaskHive.Domain.Repositories;
using TaskHive.Application.DTOs.Projects;

namespace TaskHive.Application.UseCases.Projects;

public class ListProjectsUseCase(IProjectRepository projectRepository)
{
    public async Task<PaginatedResult<ProjectDto>> ExecuteAsync(
        Guid ownerId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var projects = await projectRepository.GetByOwnerIdAsync(
            ownerId,
            pageNumber,
            pageSize,
            cancellationToken);

        var projectDtos = projects.Items.Select(p => new ProjectDto
        {
            Id = p.Id,
            OwnerId = p.OwnerId,
            Title = p.Title,
            Description = p.Description,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            ProjectStatus = p.ProjectStatus,
            ProjectType = p.ProjectType,
            CompletedAt = p.CompletedAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return new PaginatedResult<ProjectDto>(
            projectDtos,
            projects.PageNumber,
            projects.PageSize,
            projects.TotalCount);
    }
} 