using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApprenticeApp.Core.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApprenticeDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Apprentices.AnyAsync(cancellationToken))
        {
            return;
        }

        var mentors = new[]
        {
            new Mentor { Name = "Alex Morgan", Email = "alex.morgan@example.com" },
            new Mentor { Name = "Priya Desai", Email = "priya.desai@example.com" },
            new Mentor { Name = "Sam Lee", Email = "sam.lee@example.com" }
        };

        var apprentices = new[]
        {
            new Apprentice
            {
                FirstName = "Jordan",
                LastName = "Kim",
                Email = "jordan.kim@example.com",
                StartDate = DateTime.UtcNow.AddMonths(-2),
                Track = ApprenticeTrack.Engineering,
                Status = ApprenticeStatus.Active
            },
            new Apprentice
            {
                FirstName = "Taylor",
                LastName = "Patel",
                Email = "taylor.patel@example.com",
                StartDate = DateTime.UtcNow.AddMonths(-1),
                Track = ApprenticeTrack.Data,
                Status = ApprenticeStatus.Active
            },
            new Apprentice
            {
                FirstName = "Morgan",
                LastName = "Chen",
                Email = "morgan.chen@example.com",
                StartDate = DateTime.UtcNow.AddMonths(-3),
                Track = ApprenticeTrack.Design,
                Status = ApprenticeStatus.OnLeave
            }
        };

        await context.Mentors.AddRangeAsync(mentors, cancellationToken);
        await context.Apprentices.AddRangeAsync(apprentices, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var assignments = new[]
        {
            new Assignment
            {
                ApprenticeId = apprentices[0].Id,
                MentorId = mentors[0].Id,
                Title = "Build onboarding API",
                DueDate = DateTime.UtcNow.AddDays(10),
                Status = AssignmentStatus.InProgress,
                Notes = "Focus on clean contracts and unit tests."
            },
            new Assignment
            {
                ApprenticeId = apprentices[0].Id,
                MentorId = mentors[1].Id,
                Title = "Data modeling review",
                DueDate = DateTime.UtcNow.AddDays(20),
                Status = AssignmentStatus.NotStarted
            },
            new Assignment
            {
                ApprenticeId = apprentices[1].Id,
                MentorId = mentors[2].Id,
                Title = "Dashboard prototype",
                DueDate = DateTime.UtcNow.AddDays(5),
                Status = AssignmentStatus.NotStarted
            }
        };

        await context.Assignments.AddRangeAsync(assignments, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
