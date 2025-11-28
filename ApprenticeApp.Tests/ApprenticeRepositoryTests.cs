using ApprenticeApp.Core.Data;
using ApprenticeApp.Core.Entities;
using ApprenticeApp.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApprenticeApp.Tests;

public class ApprenticeRepositoryTests
{
    private static ApprenticeDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApprenticeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApprenticeDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistApprentice()
    {
        using var context = CreateContext(nameof(AddAsync_ShouldPersistApprentice));
        var repo = new ApprenticeRepository(context);

        var apprentice = new Apprentice
        {
            FirstName = "Casey",
            LastName = "Ramos",
            Email = "casey@example.com",
            StartDate = DateTime.UtcNow.Date,
            Track = ApprenticeTrack.Engineering,
            Status = ApprenticeStatus.Active
        };

        var created = await repo.AddAsync(apprentice);

        Assert.True(created.Id > 0);
        Assert.Equal(1, await context.Apprentices.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveApprenticeAndAssignments()
    {
        using var context = CreateContext(nameof(DeleteAsync_ShouldRemoveApprenticeAndAssignments));
        var repo = new ApprenticeRepository(context);

        var apprentice = new Apprentice
        {
            FirstName = "Jordan",
            LastName = "Kim",
            Email = "jordan@example.com",
            StartDate = DateTime.UtcNow.Date,
            Track = ApprenticeTrack.Engineering,
            Status = ApprenticeStatus.Active,
            Assignments = new List<Assignment>
            {
                new Assignment
                {
                    Title = "Test",
                    Status = AssignmentStatus.NotStarted,
                    Mentor = new Mentor { Name = "Mentor", Email = "mentor@example.com" }
                }
            }
        };

        context.Apprentices.Add(apprentice);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(apprentice.Id);

        Assert.Equal(0, await context.Apprentices.CountAsync());
        Assert.Equal(0, await context.Assignments.CountAsync()); // cascade delete expected
    }
}
