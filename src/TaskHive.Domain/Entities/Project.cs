using TaskHive.Domain.Enums;
using TaskHive.Domain.Exceptions;

namespace TaskHive.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public ProjectStatus ProjectStatus { get; private set; }
    public ProjectType ProjectType { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }


    // Private constructor to force use of factory method
    private Project(
        Guid id,
        Guid ownerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        ProjectStatus projectStatus,
        ProjectType projectType,
        DateTime? completedAt,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        ValidateTitle(title);
        ValidateDates(startDate, endDate);

        Id = id;
        OwnerId = ownerId;
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        ProjectStatus = projectStatus;
        ProjectType = projectType;
        CompletedAt = completedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    // Factory method for creation
    public static Project Create(
        Guid ownerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        ProjectType projectType
    )
    {
        return new Project(
            Guid.NewGuid(),
            ownerId,
            title,
            description,
            startDate,
            endDate,
            ProjectStatus.NOT_STARTED,
            projectType,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow
        );
    }

    // Factory method for reconstruction
    public static Project Load(
        Guid id,
        Guid ownerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        ProjectStatus status,
        ProjectType projectType,
        DateTime? completedAt,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        return new Project(
            id,
            ownerId,
            title,
            description,
            startDate,
            endDate,
            status,
            projectType,
            completedAt,
            createdAt,
            updatedAt
        );
    }

    public void UpdateStatus(ProjectStatus newStatus)
    {
        if (ProjectStatus == ProjectStatus.COMPLETED)
            throw new DomainException("Cannot update status of a completed project.");

        ProjectStatus = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDates(DateTime newStartDate, DateTime newEndDate)
    {
        ValidateDates(newStartDate, newEndDate);

        StartDate = newStartDate;
        EndDate = newEndDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (ProjectStatus == ProjectStatus.COMPLETED)
            throw new DomainException("Project is already completed.");

        ProjectStatus = ProjectStatus.COMPLETED;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (ProjectStatus == ProjectStatus.COMPLETED)
            throw new DomainException("Cannot cancel a completed project.");

        if (ProjectStatus == ProjectStatus.CANCELLED)
            throw new DomainException("Project is already cancelled.");

        ProjectStatus = ProjectStatus.CANCELLED;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Project title cannot be empty.");
    }

    private static void ValidateDates(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");
    }
} 