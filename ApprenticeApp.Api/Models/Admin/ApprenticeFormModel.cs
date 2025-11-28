using System.ComponentModel.DataAnnotations;
using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Api.Models.Admin;

public class ApprenticeFormModel
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    public ApprenticeTrack Track { get; set; }

    [Required]
    public ApprenticeStatus Status { get; set; } = ApprenticeStatus.Active;
}
