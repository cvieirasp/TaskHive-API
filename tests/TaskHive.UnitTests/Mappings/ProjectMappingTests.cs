using FluentAssertions;
using Moq;
using System.Data;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Enums;
using TaskHive.Infrastructure.Mappings;

namespace TaskHive.UnitTests.Mappings;

public class ProjectMappingTests
{
    private readonly Project _testProject;
    private readonly Mock<IDataReader> _readerMock;
    private readonly Mock<IDbCommand> _commandMock;
    private readonly User _owner;

    public ProjectMappingTests()
    {
        _owner = User.CreateWithEmailAndPassword(
            "owner@example.com",
            "hashed_password",
            "John",
            "Doe"
        );

        _testProject = Project.Create(
            ownerId: _owner.Id,
            title: "Test Project",
            description: "Test Description",
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddMonths(1),
            projectType: ProjectType.PROFESSIONAL
        );

        _readerMock = new Mock<IDataReader>();
        _commandMock = new Mock<IDbCommand>();
    }

    [Fact]
    public void MapFromReader_WithValidData_ShouldMapCorrectly()
    {
        // Arrange
        SetupReaderMock(_readerMock);

        // Act
        var result = ProjectMapping.MapFromReader(_readerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testProject.Id);
        result.OwnerId.Should().Be(_testProject.OwnerId);
        result.Title.Should().Be(_testProject.Title);
        result.Description.Should().Be(_testProject.Description);
        result.StartDate.Should().Be(_testProject.StartDate);
        result.EndDate.Should().Be(_testProject.EndDate);
        result.ProjectStatus.Should().Be(_testProject.ProjectStatus);
        result.ProjectType.Should().Be(_testProject.ProjectType);
        result.CompletedAt.Should().BeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MapFromReader_WithCompletedProject_ShouldMapCorrectly()
    {
        // Arrange
        var completedProject = Project.Create(
            ownerId: _owner.Id,
            title: "Completed Project",
            description: "Completed Description",
            startDate: DateTime.UtcNow.AddMonths(-1),
            endDate: DateTime.UtcNow,
            projectType: ProjectType.PROFESSIONAL
        );
        completedProject.Complete();

        SetupReaderMock(_readerMock, completedProject);

        // Act
        var result = ProjectMapping.MapFromReader(_readerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.ProjectStatus.Should().Be(ProjectStatus.COMPLETED);
        result.CompletedAt.Should().NotBeNull();
        result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddParameters_WithValidProject_ShouldAddAllParameters()
    {
        // Arrange
        var parameters = new List<IDataParameter>();
        _commandMock.Setup(x => x.Parameters.Add(It.IsAny<IDataParameter>()))
            .Callback<object>((obj) => parameters.Add((IDataParameter)obj));

        // Act
        ProjectMapping.AddParameters(_commandMock.Object, _testProject);

        // Assert
        parameters.Should().HaveCount(11);
        parameters.Should().Contain(p => p.ParameterName == "Id" && p.Value!.Equals(_testProject.Id));
        parameters.Should().Contain(p => p.ParameterName == "OwnerId" && p.Value!.Equals(_testProject.OwnerId));
        parameters.Should().Contain(p => p.ParameterName == "Title" && p.Value!.Equals(_testProject.Title));
        parameters.Should().Contain(p => p.ParameterName == "Description" && p.Value!.Equals(_testProject.Description));
        parameters.Should().Contain(p => p.ParameterName == "StartDate" && p.Value!.Equals(_testProject.StartDate));
        parameters.Should().Contain(p => p.ParameterName == "EndDate" && p.Value!.Equals(_testProject.EndDate));
        parameters.Should().Contain(p => p.ParameterName == "ProjectStatus" && p.Value!.Equals(_testProject.ProjectStatus.ToString()));
        parameters.Should().Contain(p => p.ParameterName == "ProjectType" && p.Value!.Equals(_testProject.ProjectType.ToString()));
        parameters.Should().Contain(p => p.ParameterName == "CompletedAt" && p.Value!.Equals(DBNull.Value));
        parameters.Should().Contain(p => p.ParameterName == "CreatedAt" && p.Value!.Equals(_testProject.CreatedAt));
        parameters.Should().Contain(p => p.ParameterName == "UpdatedAt" && p.Value!.Equals(_testProject.UpdatedAt));
    }

    [Fact]
    public void AddParameters_WithCompletedProject_ShouldAddCompletedAtParameter()
    {
        // Arrange
        var completedProject = Project.Create(
            ownerId: _owner.Id,
            title: "Completed Project",
            description: "Completed Description",
            startDate: DateTime.UtcNow.AddMonths(-1),
            endDate: DateTime.UtcNow,
            projectType: ProjectType.PROFESSIONAL
        );
        completedProject.Complete();

        var parameters = new List<IDataParameter>();
        _commandMock.Setup(x => x.Parameters.Add(It.IsAny<IDataParameter>()))
            .Callback<object>((obj) => parameters.Add((IDataParameter)obj));

        // Act
        ProjectMapping.AddParameters(_commandMock.Object, completedProject);

        // Assert
        parameters.Should().Contain(p => p.ParameterName == "CompletedAt" && p.Value!.Equals(completedProject.CompletedAt));
    }

    private void SetupReaderMock(Mock<IDataReader> readerMock, Project? project = null)
    {
        project ??= _testProject;

        readerMock.Setup(x => x.GetOrdinal("id")).Returns(0);
        readerMock.Setup(x => x.GetOrdinal("owner_id")).Returns(1);
        readerMock.Setup(x => x.GetOrdinal("title")).Returns(2);
        readerMock.Setup(x => x.GetOrdinal("description")).Returns(3);
        readerMock.Setup(x => x.GetOrdinal("start_date")).Returns(4);
        readerMock.Setup(x => x.GetOrdinal("end_date")).Returns(5);
        readerMock.Setup(x => x.GetOrdinal("project_status")).Returns(6);
        readerMock.Setup(x => x.GetOrdinal("project_type")).Returns(7);
        readerMock.Setup(x => x.GetOrdinal("completed_at")).Returns(8);
        readerMock.Setup(x => x.GetOrdinal("created_at")).Returns(9);
        readerMock.Setup(x => x.GetOrdinal("updated_at")).Returns(10);

        readerMock.Setup(x => x.GetGuid(0)).Returns(project.Id);
        readerMock.Setup(x => x.GetGuid(1)).Returns(project.OwnerId);
        readerMock.Setup(x => x.GetString(2)).Returns(project.Title);
        readerMock.Setup(x => x.GetString(3)).Returns(project.Description);
        readerMock.Setup(x => x.GetDateTime(4)).Returns(project.StartDate);
        readerMock.Setup(x => x.GetDateTime(5)).Returns(project.EndDate);
        readerMock.Setup(x => x.GetString(6)).Returns(project.ProjectStatus.ToString());
        readerMock.Setup(x => x.GetString(7)).Returns(project.ProjectType.ToString());
        readerMock.Setup(x => x.GetDateTime(9)).Returns(project.CreatedAt);
        readerMock.Setup(x => x.GetDateTime(10)).Returns(project.UpdatedAt);

        // Handle nullable completed_at
        readerMock.Setup(x => x.IsDBNull(8)).Returns(project.CompletedAt == null);
        if (project.CompletedAt != null)
            readerMock.Setup(x => x.GetDateTime(8)).Returns(project.CompletedAt.Value);
    }
} 