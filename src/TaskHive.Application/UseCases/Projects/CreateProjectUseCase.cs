using TaskHive.Domain.Entities;
using TaskHive.Domain.Enums;
using TaskHive.Domain.Repositories;
using TaskHive.Domain.Exceptions;

namespace TaskHive.Application.UseCases.Projects;

public class CreateProjectUseCase(IProjectRepository projectRepository)
{
    public async Task<Project> ExecuteAsync(
        Guid ownerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        ProjectType projectType = ProjectType.PERSONAL,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Project title cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Project description cannot be empty.");

        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");

        // Create project
        var project = Project.Create(
            ownerId,
            title,
            description,
            startDate,
            endDate,
            projectType
        );

        // Save to repository
        return await projectRepository.AddAsync(project, cancellationToken);
    }
} 