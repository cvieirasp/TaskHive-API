using System.ComponentModel.DataAnnotations;
using TaskHive.Domain.Enums;

namespace TaskHive.Application.DTOs.Projects;

public class CreateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public ProjectType ProjectType { get; set; } = ProjectType.PERSONAL;
} 