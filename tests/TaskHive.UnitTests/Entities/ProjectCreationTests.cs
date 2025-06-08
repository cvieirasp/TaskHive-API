using FluentAssertions;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Exceptions;
using TaskHive.Domain.Enums;

namespace TaskHive.UnitTests.Entities;

public class ProjectCreationTests
{
    private readonly User _owner;

    public ProjectCreationTests()
    {
        _owner = User.CreateWithEmailAndPassword(
            email: "owner@example.com",
            passwordHash: "hashed_password_123",
            firstName: "John",
            lastName: "Doe"
        );
    }

    [Fact]
    public void CreateProject_WithValidData_ShouldSucceed()
    {
        // Arrange
        var title = "Task Management System";
        var description = "A comprehensive task management solution";
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(3);
        var projectType = ProjectType.Professional;

        // Act
        var project = Project.Create(
            ownerId: _owner.Id,
            title: title,
            description: description,
            startDate: startDate,
            endDate: endDate,
            projectType: projectType
        );

        // Assert
        project.Id.Should().NotBe(Guid.Empty);
        project.Title.Should().Be(title);
        project.Description.Should().Be(description);
        project.OwnerId.Should().Be(_owner.Id);
        project.StartDate.Should().Be(startDate);
        project.EndDate.Should().Be(endDate);
        project.ProjectType.Should().Be(projectType);
        project.ProjectStatus.Should().Be(ProjectStatus.NotStarted);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateProject_WithInvalidTitle_ShouldThrowException(string invalidTitle)
    {
        // Arrange
        var description = "A comprehensive task management solution";
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(3);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            Project.Create(
                ownerId: _owner.Id,
                title: invalidTitle,
                description: description,
                startDate: startDate,
                endDate: endDate,
                projectType: ProjectType.Professional
            )
        );

        exception.Message.Should().Be("Project title cannot be empty.");
    }

    [Fact]
    public void CreateProject_WithEndDateBeforeStartDate_ShouldThrowException()
    {
        // Arrange
        var title = "Task Management System";
        var description = "A comprehensive task management solution";
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            Project.Create(
                ownerId: _owner.Id,
                title: title,
                description: description,
                startDate: startDate,
                endDate: endDate,
                projectType: ProjectType.Professional
            )
        );

        exception.Message.Should().Be("End date must be after start date.");
    }

    [Fact]
    public void UpdateProjectStatus_ShouldUpdateStatusAndUpdatedAt()
    {
        // Arrange
        var project = Project.Create(
            ownerId: _owner.Id,
            title: "Task Management System",
            description: "A comprehensive task management solution",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(3),
            projectType: ProjectType.Professional
        );

        // Act
        project.UpdateStatus(ProjectStatus.InProgress);

        // Assert
        project.ProjectStatus.Should().Be(ProjectStatus.InProgress);
        project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateProjectDates_WithValidDates_ShouldSucceed()
    {
        // Arrange
        var project = Project.Create(
            ownerId: _owner.Id,
            title: "Task Management System",
            description: "A comprehensive task management solution",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(3),
            projectType: ProjectType.Professional
        );

        var newStartDate = DateTime.UtcNow.AddDays(1);
        var newEndDate = newStartDate.AddMonths(2);

        // Act
        project.UpdateDates(newStartDate, newEndDate);

        // Assert
        project.StartDate.Should().Be(newStartDate);
        project.EndDate.Should().Be(newEndDate);
        project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateProjectDates_WithInvalidDates_ShouldThrowException()
    {
        // Arrange
        var project = Project.Create(
            ownerId: _owner.Id,
            title: "Task Management System",
            description: "A comprehensive task management solution",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(3),
            projectType: ProjectType.Professional
        );

        var newStartDate = DateTime.UtcNow.AddDays(1);
        var newEndDate = newStartDate.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => 
            project.UpdateDates(newStartDate, newEndDate)
        );

        exception.Message.Should().Be("End date must be after start date.");
    }

    [Fact]
    public void CompleteProject_ShouldUpdateStatusAndCompletionDate()
    {
        // Arrange
        var project = Project.Create(
            ownerId: _owner.Id,
            title: "Task Management System",
            description: "A comprehensive task management solution",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(3),
            projectType: ProjectType.Professional
        );

        // Act
        project.Complete();

        // Assert
        project.ProjectStatus.Should().Be(ProjectStatus.Completed);
        project.CompletedAt.Should().NotBeNull();
        project.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompleteProject_WhenAlreadyCompleted_ShouldThrowException()
    {
        // Arrange
        var project = Project.Create(
            ownerId: _owner.Id,
            title: "Task Management System",
            description: "A comprehensive task management solution",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(3),
            projectType: ProjectType.Professional
        );

        project.Complete();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => project.Complete());
        exception.Message.Should().Be("Project is already completed.");
    }
} 