using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TaskHive.Application.DTOs;
using TaskHive.Application.UseCases.Users;

namespace TaskHive.IntegrationTests.Auth;

public class AuthControllerTests : TestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestIpAddress = "127.0.0.1";
    private const string TestEmail = "test@example.com";
    private const string TestPassword = "Test123!";

    private Guid _userId;

    public AuthControllerTests(WebApplicationFactory<Program> factory) : base()
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.test.json", optional: false);
            });
        });

        Client = ConfigureClient(_factory, client =>
        {
            client.DefaultRequestHeaders.Add("X-Forwarded-For", TestIpAddress);
            return client;
        });
    }

    public async Task InitializeAsync()
    {
        var signUpUseCase = ServiceScope.ServiceProvider.GetRequiredService<SignUpUseCase>();
        var user = await signUpUseCase.ExecuteAsync(TestEmail, TestPassword, "Test", "User");
        _userId = user.Id;
    }

    public async Task DisposeAsync()
    {
        var deleteUserUseCase = ServiceScope.ServiceProvider.GetRequiredService<DeleteUserUseCase>();
        await deleteUserUseCase.ExecuteAsync(_userId);
    }

    [Fact]
    public async Task SignIn_WithinRateLimit_ShouldSucceed()
    {
        // Arrange
        var request = new SignInRequest { Email = TestEmail, Password = TestPassword };

        // Act
        var response = await Client.PostAsJsonAsync("/api/Auth/signin", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SignIn_ExceedingRateLimit_ShouldReturn429()
    {
        // Arrange
        var request = new SignInRequest { Email = TestEmail, Password = TestPassword };

        // Act - Make 4 requests (exceeding limit of 3)
        for (int i = 0; i < 4; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/Auth/signin", request);
            
            if (i < 3)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            else
            {
                Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
                
                // Verify rate limiting headers
                Assert.True(response.Headers.Contains("Retry-After"), "Retry-After header should be present");
                var retryAfter = response.Headers.GetValues("Retry-After").First();
                Assert.True(int.TryParse(retryAfter, out _), "Retry-After header should be a valid number");
                
                // Verify response body
                var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                Assert.NotNull(content);
                Assert.Equal("Too many requests.", content.Title);
            }
        }
    }

    [Fact]
    public async Task SignIn_AfterRateLimitWindow_ShouldSucceed()
    {
        // Arrange
        var request = new SignInRequest { Email = TestEmail, Password = TestPassword };

        // Act - Make 3 requests (reaching limit)
        for (int i = 0; i < 3; i++)
        {
            await Client.PostAsJsonAsync("/api/Auth/signin", request);
        }

        // Wait for window period to expire
        await Task.Delay(TimeSpan.FromMinutes(1));

        // Try again
        var response = await Client.PostAsJsonAsync("/api/Auth/signin", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
} 