using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Api.Dtos;

public record ApprenticeDto(
    int Id,
    string Name,
    string Email,
    ApprenticeTrack Track,
    ApprenticeStatus Status,
    DateTime StartDate);
