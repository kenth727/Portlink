using System.ComponentModel.DataAnnotations;
using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Api.Requests;

public class ApprenticeRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public ApprenticeTrack Track { get; set; }

    [Required]
    public ApprenticeStatus Status { get; set; }
}
