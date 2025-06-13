using FluentAssertions;
using Moq;
using TaskHive.Domain.Common;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Enums;
using TaskHive.Domain.Repositories;

namespace TaskHive.UnitTests.Repositories;

public class ProjectRepositoryTests
{
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly IProjectRepository _repository;
    private readonly User _owner;

    public ProjectRepositoryTests()
    {
        _repositoryMock = new Mock<IProjectRepository>();
        _repository = _repositoryMock.Object;
        _owner = User.CreateWithEmailAndPassword(
            email: "owner@example.com",
            passwordHash: "hashed_password_123",
            firstName: "John",
            lastName: "Doe"
        );
    }

    [Fact]
    public async Task AddProject_ShouldSucceed()
    {
        // Arrange
        var project = Project.Create(
            ownerId: _owner.Id,
            title: "Test Project",
            description: "Test Description",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(1),
            projectType: ProjectType.PROFESSIONAL
        );

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await _repository.AddAsync(project);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Title.Should().Be(project.Title);
        result.OwnerId.Should().Be(_owner.Id);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_WithExistingProject_ShouldReturnProject()
    {
        // Arrange
        var project = Project.Create(
            ownerId: _owner.Id,
            title: "Test Project",
            description: "Test Description",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(1),
            ProjectType.PROFESSIONAL
        );

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await _repository.GetByIdAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(project.Id);
        result.Title.Should().Be(project.Title);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistingProject_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByOwnerId_ShouldReturnPaginatedOwnerProjects()
    {
        // Arrange
        var projects = new List<Project>
        {
            Project.Create(
                ownerId: _owner.Id,
                title: "Project 1",
                description: "Description 1",
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddMonths(1),
                projectType: ProjectType.PROFESSIONAL
            ),
            Project.Create(
                ownerId: _owner.Id,
                title: "Project 2",
                description: "Description 2",
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddMonths(2),
                projectType: ProjectType.PERSONAL
            )
        };

        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 2;

        var paginatedResult = new PaginatedResult<Project>(projects, pageNumber, pageSize, totalCount);

        _repositoryMock.Setup(r => r.GetByOwnerIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _repository.GetByOwnerIdAsync(_owner.Id, pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.OwnerId.Should().Be(_owner.Id));
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().Be(totalCount);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
        _repositoryMock.Verify(r => r.GetByOwnerIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Exists_WithExistingProject_ShouldReturnTrue()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Exists_WithNonExistingProject_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
} 