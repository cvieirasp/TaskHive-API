using Moq;
using TaskHive.Domain.Entities;
using TaskHive.Domain.Repositories;
using TaskHive.Domain.Services;
using TaskHive.Application.UseCases.Users;
using TaskHive.Application.Exceptions;

namespace TaskHive.UnitTests.UseCases.Users;

public class SignUpUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly SignUpUseCase _signUpUseCase;

    public SignUpUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _signUpUseCase = new SignUpUseCase(_userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldCreateUserWithEmailAndPassword()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var password = "TestPassword123!";
        var firstName = "John";
        var lastName = "Doe";
        var passwordHash = "hashed_password";
        var user = User.CreateWithEmailAndPassword(email, passwordHash, firstName, lastName);

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.HashPassword(password))
            .Returns(passwordHash);
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _signUpUseCase.ExecuteAsync(email, password, firstName, lastName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(passwordHash, result.PasswordHash);
        Assert.Equal(firstName, result.FirstName);
        Assert.Equal(lastName, result.LastName);

        _userRepositoryMock.Verify(x => x.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(password), Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingEmail_ShouldThrowEmailAlreadyInUseException()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123!";
        var firstName = "John";
        var lastName = "Doe";

        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() =>
            _signUpUseCase.ExecuteAsync(email, password, firstName, lastName));

        _userRepositoryMock.Verify(x => x.ExistsByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
} 