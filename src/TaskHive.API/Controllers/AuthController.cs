using Microsoft.AspNetCore.Mvc;
using TaskHive.Application.DTOs;
using TaskHive.Application.UseCases.Users;

namespace TaskHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    SignInUseCase signInUseCase,
    SignUpUseCase signUpUseCase,
    SendVerificationEmailUseCase sendVerificationEmailUseCase,
    VerifyEmailUseCase verifyEmailUseCase) : ControllerBase
{
    [HttpPost("signup")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request, CancellationToken cancellationToken)
    {
        var user = await signUpUseCase.ExecuteAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);

        await sendVerificationEmailUseCase.ExecuteAsync(user.Id, cancellationToken);

        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailVerified = user.IsEmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return CreatedAtAction(
            nameof(SignUp),
            new { id = user.Id },
            response);
    }

    [HttpPost("signin")]
    [ProducesResponseType(typeof(SignInResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
    {
        var response = await signInUseCase.ExecuteAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("send-verification-email")]
    public async Task<IActionResult> SendVerificationEmail(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value!);
        await sendVerificationEmailUseCase.ExecuteAsync(userId, cancellationToken);

        return Ok(new { message = "Verification email sent successfully." });
    }

    [HttpPost("validate-email")]
    public async Task<IActionResult> ValidateEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        await verifyEmailUseCase.ExecuteAsync(token, cancellationToken);

        return Ok(new { message = "Email verified successfully." });
    }
} 