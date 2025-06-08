using TaskHive.Domain.Common;
using TaskHive.Domain.Entities;

namespace TaskHive.Domain.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Project>> GetByOwnerIdAsync(Guid ownerId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);
} 