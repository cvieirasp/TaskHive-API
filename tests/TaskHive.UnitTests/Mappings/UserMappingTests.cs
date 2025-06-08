using FluentAssertions;
using Moq;
using System.Data;
using TaskHive.Domain.Entities;
using TaskHive.Infrastructure.Mappings;

namespace TaskHive.UnitTests.Mappings;

public class UserMappingTests
{
    private readonly User _testUser;
    private readonly Mock<IDataReader> _readerMock;
    private readonly Mock<IDbCommand> _commandMock;

    public UserMappingTests()
    {
        _testUser = User.CreateWithEmailAndPassword(
            "test@example.com",
            "hashed_password",
            "John",
            "Doe"
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
        var result = UserMapping.MapFromReader(_readerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(_testUser.Id);
        result.Email.Should().Be(_testUser.Email);
        result.PasswordHash.Should().Be(_testUser.PasswordHash);
        result.FirstName.Should().Be(_testUser.FirstName);
        result.LastName.Should().Be(_testUser.LastName);
        result.IsEmailVerified.Should().BeFalse();
        result.TwoFactorEnabled.Should().BeFalse();
        result.IsActive.Should().BeTrue();
        result.OAuthProvider.Should().BeNull();
        result.OAuthId.Should().BeNull();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MapFromReader_WithOAuthData_ShouldMapCorrectly()
    {
        // Arrange
        var oauthUser = User.CreateWithOAuth(
            "test@example.com",
            "John",
            "Doe",
            "Google",
            "oauth123"
        );

        SetupReaderMock(_readerMock, oauthUser);

        // Act
        var result = UserMapping.MapFromReader(_readerMock.Object);

        // Assert
        result.Should().NotBeNull();
        result.OAuthProvider.Should().Be("Google");
        result.OAuthId.Should().Be("oauth123");
        result.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void AddParameters_WithValidUser_ShouldAddAllParameters()
    {
        // Arrange
        var parameters = new List<IDataParameter>();
        _commandMock.Setup(x => x.Parameters.Add(It.IsAny<IDataParameter>()))
            .Callback<object>((obj) => parameters.Add((IDataParameter)obj));

        // Act
        UserMapping.AddParameters(_commandMock.Object, _testUser);

        // Assert
        parameters.Should().HaveCount(12);
        parameters.Should().Contain(p => p.ParameterName == "Id" && p.Value!.Equals(_testUser.Id));
        parameters.Should().Contain(p => p.ParameterName == "Email" && p.Value!.Equals(_testUser.Email.ToLowerInvariant()));
        parameters.Should().Contain(p => p.ParameterName == "PasswordHash" && p.Value!.Equals(_testUser.PasswordHash));
        parameters.Should().Contain(p => p.ParameterName == "FirstName" && p.Value!.Equals(_testUser.FirstName));
        parameters.Should().Contain(p => p.ParameterName == "LastName" && p.Value!.Equals(_testUser.LastName));
        parameters.Should().Contain(p => p.ParameterName == "IsEmailVerified" && p.Value!.Equals(_testUser.IsEmailVerified));
        parameters.Should().Contain(p => p.ParameterName == "TwoFactorEnabled" && p.Value!.Equals(_testUser.TwoFactorEnabled));
        parameters.Should().Contain(p => p.ParameterName == "IsActive" && p.Value!.Equals(_testUser.IsActive));
        parameters.Should().Contain(p => p.ParameterName == "OAuthProvider" && p.Value!.Equals(DBNull.Value));
        parameters.Should().Contain(p => p.ParameterName == "OAuthId" && p.Value!.Equals(DBNull.Value));
        parameters.Should().Contain(p => p.ParameterName == "CreatedAt" && p.Value!.Equals(_testUser.CreatedAt));
        parameters.Should().Contain(p => p.ParameterName == "UpdatedAt" && p.Value!.Equals(_testUser.UpdatedAt));
    }

    [Fact]
    public void AddParameters_WithOAuthUser_ShouldAddOAuthParameters()
    {
        // Arrange
        var oauthUser = User.CreateWithOAuth(
            "test@example.com",
            "John",
            "Doe",
            "Google",
            "oauth123"
        );

        var parameters = new List<IDataParameter>();
        _commandMock.Setup(x => x.Parameters.Add(It.IsAny<IDataParameter>()))
            .Callback<object>((obj) => parameters.Add((IDataParameter)obj));

        // Act
        UserMapping.AddParameters(_commandMock.Object, oauthUser);

        // Assert
        parameters.Should().Contain(p => p.ParameterName == "OAuthProvider" && p.Value!.Equals("Google"));
        parameters.Should().Contain(p => p.ParameterName == "OAuthId" && p.Value!.Equals("oauth123"));
        parameters.Should().Contain(p => p.ParameterName == "PasswordHash" && p.Value!.Equals(DBNull.Value));
    }

    private void SetupReaderMock(Mock<IDataReader> readerMock, User? user = null)
    {
        user ??= _testUser;

        readerMock.Setup(x => x.GetOrdinal("id")).Returns(0);
        readerMock.Setup(x => x.GetOrdinal("email")).Returns(1);
        readerMock.Setup(x => x.GetOrdinal("password_hash")).Returns(2);
        readerMock.Setup(x => x.GetOrdinal("first_name")).Returns(3);
        readerMock.Setup(x => x.GetOrdinal("last_name")).Returns(4);
        readerMock.Setup(x => x.GetOrdinal("is_email_verified")).Returns(5);
        readerMock.Setup(x => x.GetOrdinal("two_factor_enabled")).Returns(6);
        readerMock.Setup(x => x.GetOrdinal("is_active")).Returns(7);
        readerMock.Setup(x => x.GetOrdinal("oauth_provider")).Returns(8);
        readerMock.Setup(x => x.GetOrdinal("oauth_id")).Returns(9);
        readerMock.Setup(x => x.GetOrdinal("created_at")).Returns(10);
        readerMock.Setup(x => x.GetOrdinal("updated_at")).Returns(11);

        readerMock.Setup(x => x.GetGuid(0)).Returns(user.Id);
        readerMock.Setup(x => x.GetString(1)).Returns(user.Email);
        readerMock.Setup(x => x.GetString(3)).Returns(user.FirstName);
        readerMock.Setup(x => x.GetString(4)).Returns(user.LastName);
        readerMock.Setup(x => x.GetBoolean(5)).Returns(user.IsEmailVerified);
        readerMock.Setup(x => x.GetBoolean(6)).Returns(user.TwoFactorEnabled);
        readerMock.Setup(x => x.GetBoolean(7)).Returns(user.IsActive);
        readerMock.Setup(x => x.GetDateTime(10)).Returns(user.CreatedAt);
        readerMock.Setup(x => x.GetDateTime(11)).Returns(user.UpdatedAt);

        // Handle nullable fields
        readerMock.Setup(x => x.IsDBNull(2)).Returns(user.PasswordHash == null);
        readerMock.Setup(x => x.IsDBNull(8)).Returns(user.OAuthProvider == null);
        readerMock.Setup(x => x.IsDBNull(9)).Returns(user.OAuthId == null);

        if (user.PasswordHash != null)
            readerMock.Setup(x => x.GetString(2)).Returns(user.PasswordHash);
        if (user.OAuthProvider != null)
            readerMock.Setup(x => x.GetString(8)).Returns(user.OAuthProvider);
        if (user.OAuthId != null)
            readerMock.Setup(x => x.GetString(9)).Returns(user.OAuthId);
    }
} 