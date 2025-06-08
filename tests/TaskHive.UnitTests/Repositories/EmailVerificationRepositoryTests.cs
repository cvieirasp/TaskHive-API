using FluentAssertions;
using Moq;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;

namespace TaskHive.UnitTests.Repositories;

public class EmailVerificationRepositoryTests
{
    private readonly Mock<IEmailVerificationRepository> _repositoryMock;
    private readonly IEmailVerificationRepository _repository;
    private readonly EmailVerificationToken _testToken;

    public EmailVerificationRepositoryTests()
    {
        _repositoryMock = new Mock<IEmailVerificationRepository>();
        _repository = _repositoryMock.Object;
        
        // Create a test token
        _testToken = EmailVerificationToken.Create(
            userId: Guid.NewGuid(),
            token: Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            expiresAt: DateTime.UtcNow.AddHours(24)
        );
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenExists_ShouldReturnToken()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testToken);

        // Act
        var result = await _repository.GetByTokenAsync(_testToken.Token);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testToken.Id);
        result.UserId.Should().Be(_testToken.UserId);
        result.Token.Should().Be(_testToken.Token);
        result.ExpiresAt.Should().Be(_testToken.ExpiresAt);
        result.IsUsed.Should().BeFalse();
        result.UsedAt.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailVerificationToken?)null);

        // Act
        var result = await _repository.GetByTokenAsync("non-existent-token");

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLatestByUserIdAsync_WhenTokenExists_ShouldReturnToken()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetLatestByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testToken);

        // Act
        var result = await _repository.GetLatestByUserIdAsync(_testToken.UserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testToken.Id);
        result.UserId.Should().Be(_testToken.UserId);
        result.Token.Should().Be(_testToken.Token);
        result.ExpiresAt.Should().Be(_testToken.ExpiresAt);
        result.IsUsed.Should().BeFalse();
        result.UsedAt.Should().BeNull();
        _repositoryMock.Verify(r => r.GetLatestByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLatestByUserIdAsync_WhenTokenDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetLatestByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailVerificationToken?)null);

        // Act
        var result = await _repository.GetLatestByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetLatestByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTokenAndReturnIt()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testToken);

        // Act
        var result = await _repository.AddAsync(_testToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testToken.Id);
        result.UserId.Should().Be(_testToken.UserId);
        result.Token.Should().Be(_testToken.Token);
        result.ExpiresAt.Should().Be(_testToken.ExpiresAt);
        result.IsUsed.Should().BeFalse();
        result.UsedAt.Should().BeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTokenAndReturnIt()
    {
        // Arrange
        var updatedToken = _testToken;
        updatedToken.MarkAsUsed();

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedToken);

        // Act
        var result = await _repository.UpdateAsync(updatedToken);


        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<EmailVerificationToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLatestByUserIdAsync_WhenMultipleTokensExist_ShouldReturnLatestToken()
    {
        // Arrange
        var olderToken = EmailVerificationToken.Create(
            userId: _testToken.UserId,
            token: Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            expiresAt: DateTime.UtcNow.AddHours(12)
        );

        _repositoryMock.Setup(r => r.GetLatestByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testToken);

        // Act
        var result = await _repository.GetLatestByUserIdAsync(_testToken.UserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testToken.Id);
        result.ExpiresAt.Should().BeAfter(olderToken.ExpiresAt);
        _repositoryMock.Verify(r => r.GetLatestByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
} 