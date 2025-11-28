using System.ComponentModel.DataAnnotations;

namespace ApprenticeApp.Core.Entities;

public class Apprentice
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string FirstName { get; set; }

    [MaxLength(100)]
    public required string LastName { get; set; }

    [MaxLength(200)]
    public required string Email { get; set; }

    public DateTime StartDate { get; set; }

    public ApprenticeTrack Track { get; set; }

    public ApprenticeStatus Status { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
