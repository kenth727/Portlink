namespace ApprenticeApp.Api.Models.Admin;

public class ApprenticeListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Track { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AssignmentCount { get; set; }
}
