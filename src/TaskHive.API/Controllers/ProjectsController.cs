using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskHive.Application.DTOs.Projects;
using TaskHive.Application.Mappings;
using TaskHive.Application.UseCases.Projects;
using TaskHive.Domain.Common;
using TaskHive.Domain.Exceptions;

namespace TaskHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController(
    CreateProjectUseCase createProjectUseCase,
    ListProjectsUseCase listProjectsUseCase) : ControllerBase
{

    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="request">Project creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created project</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var ownerId))
                throw new DomainException("Invalid user ID in token.");

            var project = await createProjectUseCase.ExecuteAsync(
                ownerId,
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate,
                request.ProjectType,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = project.Id },
                ProjectMapping.ToDto(project));
        }
        catch (DomainException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Gets a project by its ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The project if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProjectDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                throw new DomainException("Invalid user ID in token.");

            // TODO: Implement GetProjectUseCase and inject it
            return NotFound(new ProblemDetails
            {
                Title = "Not implemented",
                Detail = "This endpoint is not yet implemented",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Lists projects for the current user
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of projects</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResult<ProjectDto>>> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var ownerId))
                throw new DomainException("Invalid user ID in token.");

            var result = await listProjectsUseCase.ExecuteAsync(
                ownerId,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
} 