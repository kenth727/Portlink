namespace ApprenticeApp.Api.Models.Admin;

public class AssignmentListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
