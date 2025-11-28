using System.ComponentModel.DataAnnotations;

namespace ApprenticeApp.Core.Entities;

public class Mentor
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public required string Email { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
