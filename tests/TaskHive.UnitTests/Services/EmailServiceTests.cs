using Moq;
using TaskHive.Domain.Services;

namespace TaskHive.UnitTests.Services;

public class EmailServiceTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly string _testEmail;
    private readonly string _testVerificationLink;
    private readonly string _testResetLink;

    public EmailServiceTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _testEmail = "test@example.com";
        _testVerificationLink = "https://taskhive.com/verify?token=test-token";
        _testResetLink = "https://taskhive.com/reset-password?token=test-token";
    }

    [Fact]
    public async Task SendVerificationEmailAsync_WithValidData_ShouldSendEmail()
    {
        // Arrange
        _emailServiceMock
            .Setup(x => x.SendVerificationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _emailServiceMock.Object.SendVerificationEmailAsync(_testEmail, _testVerificationLink);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendVerificationEmailAsync(
                _testEmail,
                _testVerificationLink,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithValidData_ShouldSendEmail()
    {
        // Arrange
        _emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _emailServiceMock.Object.SendPasswordResetEmailAsync(_testEmail, _testResetLink);

        // Assert
        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(
                _testEmail,
                _testResetLink,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
} 