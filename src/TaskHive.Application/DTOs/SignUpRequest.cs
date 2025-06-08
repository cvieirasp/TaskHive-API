using System.ComponentModel.DataAnnotations;

namespace TaskHive.Application.DTOs;

public class SignUpRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
    public string Password { get; set; } = null!;

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    [RegularExpression(@"^[A-Za-z\s-]{1,50}$",
        ErrorMessage = "First name can only contain letters, spaces, and hyphens.")]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    [RegularExpression(@"^[A-Za-z\s-]{1,50}$",
        ErrorMessage = "Last name can only contain letters, spaces, and hyphens.")]
    public string LastName { get; set; } = null!;
} 