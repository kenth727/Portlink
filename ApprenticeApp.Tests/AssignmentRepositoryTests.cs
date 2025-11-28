using ApprenticeApp.Core.Data;
using ApprenticeApp.Core.Entities;
using ApprenticeApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;

namespace ApprenticeApp.Tests;

public class AssignmentRepositoryTests
{
    private static ApprenticeDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApprenticeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApprenticeDbContext(options);
    }

    [Fact]
    public async Task GetByApprenticeAsync_ReturnsAssignmentsOrderedByDueDate()
    {
        using var context = CreateContext(nameof(GetByApprenticeAsync_ReturnsAssignmentsOrderedByDueDate));
        var repo = new AssignmentRepository(context);

        var apprentice = new Apprentice
        {
            FirstName = "Taylor",
            LastName = "Lee",
            Email = "taylor@example.com",
            StartDate = DateTime.UtcNow.Date,
            Track = ApprenticeTrack.Engineering,
            Status = ApprenticeStatus.Active
        };
        var mentor = new Mentor { Name = "Mentor", Email = "mentor@example.com" };

        context.Apprentices.Add(apprentice);
        context.Mentors.Add(mentor);
        await context.SaveChangesAsync();

        var assignments = new[]
        {
            new Assignment { ApprenticeId = apprentice.Id, MentorId = mentor.Id, Title = "B", DueDate = DateTime.UtcNow.AddDays(5), Status = AssignmentStatus.NotStarted },
            new Assignment { ApprenticeId = apprentice.Id, MentorId = mentor.Id, Title = "A", DueDate = DateTime.UtcNow.AddDays(10), Status = AssignmentStatus.NotStarted }
        };
        context.Assignments.AddRange(assignments);
        await context.SaveChangesAsync();

        var result = await repo.GetByApprenticeAsync(apprentice.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result.First().Title); // ordered by DueDate descending
    }

    [Fact]
    public async Task AddAsync_ShouldAssignId()
    {
        using var context = CreateContext(nameof(AddAsync_ShouldAssignId));
        var repo = new AssignmentRepository(context);

        var apprentice = new Apprentice
        {
            FirstName = "Sam",
            LastName = "Rowe",
            Email = "sam@example.com",
            StartDate = DateTime.UtcNow.Date,
            Track = ApprenticeTrack.Engineering,
            Status = ApprenticeStatus.Active
        };
        var mentor = new Mentor { Name = "Mentor", Email = "mentor@example.com" };
        context.Apprentices.Add(apprentice);
        context.Mentors.Add(mentor);
        await context.SaveChangesAsync();

        var assignment = new Assignment
        {
            ApprenticeId = apprentice.Id,
            MentorId = mentor.Id,
            Title = "New Assignment",
            Status = AssignmentStatus.NotStarted
        };

        var created = await repo.AddAsync(assignment);

        Assert.True(created.Id > 0);
        Assert.Equal(1, await context.Assignments.CountAsync());
    }
}
