using System.ComponentModel.DataAnnotations;

namespace ApprenticeApp.Core.Entities;

public class Assignment
{
    public int Id { get; set; }

    public int ApprenticeId { get; set; }

    public Apprentice Apprentice { get; set; } = null!;

    public int MentorId { get; set; }

    public Mentor Mentor { get; set; } = null!;

    [MaxLength(200)]
    public required string Title { get; set; }

    public DateTime? DueDate { get; set; }

    public AssignmentStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
