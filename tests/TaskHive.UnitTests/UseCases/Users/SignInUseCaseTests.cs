using Moq;
using TaskHive.Application.Exceptions;
using TaskHive.Application.UseCases.Users;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;
using TaskHive.Domain.Services;

namespace TaskHive.UnitTests.UseCases.Users;

public class SignInUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly SignInUseCase _signInUseCase;

    public SignInUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _signInUseCase = new SignInUseCase(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ShouldReturnUserAndToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var password = "TestPassword123!";
        var passwordHash = "hashed_password";

        var user = User.CreateWithEmailAndPassword(email, passwordHash, "John", "Doe");
        var token = "jwt_token_123";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(password, passwordHash))
            .Returns(true);
        _jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns(token);

        // Act
        var result = await _signInUseCase.ExecuteAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Token);
        Assert.Equal(email, result.User.Email);
        Assert.Equal(passwordHash, result.User.PasswordHash);
        Assert.Equal(token, result.Token);

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(password, passwordHash), Times.Once);
        _jwtServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var email = "nonexisting@example.com";
        var password = "TestPassword123!";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _signInUseCase.ExecuteAsync(email, password));

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword123!";
        var passwordHash = "hashed_password";
        var createdAt = DateTime.UtcNow;
        var user = User.CreateWithEmailAndPassword(email, passwordHash, "John", "Doe");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(password, passwordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _signInUseCase.ExecuteAsync(email, password));

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(password, passwordHash), Times.Once);
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }
} 