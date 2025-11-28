using ApprenticeApp.Api.Dtos;
using ApprenticeApp.Api.Requests;
using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Api.Mappers;

public static class DtoMappingExtensions
{
    public static ApprenticeDto ToDto(this Apprentice apprentice) =>
        new(
            apprentice.Id,
            $"{apprentice.FirstName} {apprentice.LastName}".Trim(),
            apprentice.Email,
            apprentice.Track,
            apprentice.Status,
            apprentice.StartDate);

    public static AssignmentDto ToDto(this Assignment assignment) =>
        new(
            assignment.Id,
            assignment.Title,
            assignment.DueDate,
            assignment.Status,
            assignment.Notes,
            assignment.Mentor?.Name ?? string.Empty);

    public static Apprentice ToEntity(this ApprenticeRequest request, int? id = null)
    {
        return new Apprentice
        {
            Id = id ?? 0,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            StartDate = request.StartDate,
            Track = request.Track,
            Status = request.Status
        };
    }
}
