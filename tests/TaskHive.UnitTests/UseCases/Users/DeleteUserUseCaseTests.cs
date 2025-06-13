using Moq;
using TaskHive.Application.UseCases.Users;
using TaskHive.Domain.Repositories;

namespace TaskHive.UnitTests.UseCases.Users;

public class DeleteUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly DeleteUserUseCase _deleteUserUseCase;

    public DeleteUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _deleteUserUseCase = new DeleteUserUseCase(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallDeleteAsyncWithCorrectId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _deleteUserUseCase.ExecuteAsync(userId);

        // Assert
        _userRepositoryMock.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
} 