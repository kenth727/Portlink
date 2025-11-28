using System.ComponentModel.DataAnnotations;
using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Api.Requests;

public class AssignmentRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    [Required]
    public AssignmentStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public int MentorId { get; set; }
}
