using FluentAssertions;
using Moq;
using TaskHive.Domain.Common;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;

namespace TaskHive.UnitTests.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly IUserRepository _repository;
    private readonly User _testUser;

    public UserRepositoryTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _repository = _repositoryMock.Object;
        
        // Create a test user
        _testUser = User.CreateWithEmailAndPassword(
            "test@example.com",
            "hashed_password",
            "John",
            "Doe"
        );
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _repository.GetByIdAsync(_testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testUser.Id);
        result.Email.Should().Be(_testUser.Email);
        result.FirstName.Should().Be(_testUser.FirstName);
        result.LastName.Should().Be(_testUser.LastName);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _repository.GetByEmailAsync(_testUser.Email);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testUser.Id);
        result.Email.Should().Be(_testUser.Email);
        result.FirstName.Should().Be(_testUser.FirstName);
        result.LastName.Should().Be(_testUser.LastName);
        _repositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _repository.GetByEmailAsync("notexists@mail.com");

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.ExistsByEmailAsync(_testUser.Email);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailDoesNotExists_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _repository.ExistsByEmailAsync(_testUser.Email);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserAndReturnIt()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _repository.AddAsync(_testUser);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testUser.Id);
        result.Email.Should().Be(_testUser.Email);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedUsers()
    {
        // Arrange
        var users = new List<User> { _testUser };
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 1;

        var paginatedResult = new PaginatedResult<User>(users, pageNumber, pageSize, totalCount);

        _repositoryMock.Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _repository.GetAllAsync(pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
        _repositoryMock.Verify(r => r.GetAllAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
} 