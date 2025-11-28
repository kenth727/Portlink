using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Api.Dtos;

public record AssignmentDto(
    int Id,
    string Title,
    DateTime? DueDate,
    AssignmentStatus Status,
    string? Notes,
    string MentorName);
