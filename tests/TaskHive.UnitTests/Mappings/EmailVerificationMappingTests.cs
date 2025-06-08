using FluentAssertions;
using Moq;
using System;
using System.Data;
using TaskHive.Domain.Entities;
using TaskHive.Infra.Mappings;

namespace TaskHive.UnitTests.Mappings;

public class EmailVerificationMappingTests
{
    private readonly EmailVerificationToken _testToken;
    private readonly Mock<IDataReader> _readerMock;
    private readonly Mock<IDbCommand> _commandMock;

    public EmailVerificationMappingTests()
    {
        _testToken = EmailVerificationToken.Create(
            userId: Guid.NewGuid(),
            token: Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            expiresAt: DateTime.UtcNow.AddHours(24)
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
        var result = EmailVerificationMapping.MapFromReader(_readerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testToken.Id);
        result.UserId.Should().Be(_testToken.UserId);
        result.Token.Should().Be(_testToken.Token);
        result.ExpiresAt.Should().Be(_testToken.ExpiresAt);
        result.IsUsed.Should().BeFalse();
        result.UsedAt.Should().BeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MapFromReader_WithUsedToken_ShouldMapCorrectly()
    {
        // Arrange
        var usedToken = EmailVerificationToken.Create(
            userId: Guid.NewGuid(),
            token: Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            expiresAt: DateTime.UtcNow.AddHours(24)
        );
        usedToken.MarkAsUsed();

        SetupReaderMock(_readerMock, usedToken);

        // Act
        var result = EmailVerificationMapping.MapFromReader(_readerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.IsUsed.Should().BeTrue();
        result.UsedAt.Should().NotBeNull();
        result.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddParameters_WithValidToken_ShouldAddAllParameters()
    {
        // Arrange
        var parameters = new List<IDataParameter>();
        _commandMock.Setup(x => x.Parameters.Add(It.IsAny<IDataParameter>()))
            .Callback<object>((obj) => parameters.Add((IDataParameter)obj));

        // Act
        EmailVerificationMapping.AddParameters(_commandMock.Object, _testToken);

        // Assert
        parameters.Should().HaveCount(7);
        parameters.Should().Contain(p => p.ParameterName == "Id" && p.Value!.Equals(_testToken.Id));
        parameters.Should().Contain(p => p.ParameterName == "UserId" && p.Value!.Equals(_testToken.UserId));
        parameters.Should().Contain(p => p.ParameterName == "Token" && p.Value!.Equals(_testToken.Token));
        parameters.Should().Contain(p => p.ParameterName == "ExpiresAt" && p.Value!.Equals(_testToken.ExpiresAt));
        parameters.Should().Contain(p => p.ParameterName == "IsUsed" && p.Value!.Equals(_testToken.IsUsed));
        parameters.Should().Contain(p => p.ParameterName == "CreatedAt" && p.Value!.Equals(_testToken.CreatedAt));
        parameters.Should().Contain(p => p.ParameterName == "UsedAt" && p.Value!.Equals(DBNull.Value));
    }

    [Fact]
    public void AddParameters_WithUsedToken_ShouldAddUsedAtParameter()
    {
        // Arrange
        var usedToken = EmailVerificationToken.Create(
            userId: Guid.NewGuid(),
            token: Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            expiresAt: DateTime.UtcNow.AddHours(24)
        );
        usedToken.MarkAsUsed();

        var parameters = new List<IDataParameter>();
        _commandMock.Setup(x => x.Parameters.Add(It.IsAny<IDataParameter>()))
            .Callback<object>((obj) => parameters.Add((IDataParameter)obj));

        // Act
        EmailVerificationMapping.AddParameters(_commandMock.Object, usedToken);

        // Assert
        parameters.Should().Contain(p => p.ParameterName == "UsedAt" && p.Value!.Equals(usedToken.UsedAt));
    }

    private void SetupReaderMock(Mock<IDataReader> readerMock, EmailVerificationToken? token = null)
    {
        token ??= _testToken;

        readerMock.Setup(x => x.GetOrdinal("id")).Returns(0);
        readerMock.Setup(x => x.GetOrdinal("user_id")).Returns(1);
        readerMock.Setup(x => x.GetOrdinal("token")).Returns(2);
        readerMock.Setup(x => x.GetOrdinal("expires_at")).Returns(3);
        readerMock.Setup(x => x.GetOrdinal("is_used")).Returns(4);
        readerMock.Setup(x => x.GetOrdinal("used_at")).Returns(5);
        readerMock.Setup(x => x.GetOrdinal("created_at")).Returns(6);

        readerMock.Setup(x => x.GetGuid(0)).Returns(token.Id);
        readerMock.Setup(x => x.GetGuid(1)).Returns(token.UserId);
        readerMock.Setup(x => x.GetString(2)).Returns(token.Token);
        readerMock.Setup(x => x.GetDateTime(3)).Returns(token.ExpiresAt);
        readerMock.Setup(x => x.GetBoolean(4)).Returns(token.IsUsed);
        readerMock.Setup(x => x.GetDateTime(6)).Returns(token.CreatedAt);

        // Handle nullable used_at
        readerMock.Setup(x => x.IsDBNull(5)).Returns(token.UsedAt == null);
        if (token.UsedAt != null)
            readerMock.Setup(x => x.GetDateTime(5)).Returns(token.UsedAt.Value);
    }
} 