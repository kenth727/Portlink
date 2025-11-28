using System.ComponentModel.DataAnnotations;
using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Api.Models.Admin;

public class AssignmentFormModel
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Required]
    public AssignmentStatus Status { get; set; } = AssignmentStatus.NotStarted;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public int MentorId { get; set; }

    public int ApprenticeId { get; set; }
}
