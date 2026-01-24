using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.StreakModule.Endpoints;

public record DayStreakStatus(
    string Day,
    bool? HasStreak);

public record GetUserStreakResponse(
    int CurrentStreakDays,
    int TotalPoints,
    List<DayStreakStatus> WeeklyStatus);

public class GetUserStreakEndpoint(ApplicationDbContext context)
    : EndpointWithoutRequest<ApiResponse<GetUserStreakResponse>>
{
    public override void Configure()
    {
        Get("/me");
        Group<StreakEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get streak logs for current week
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek); // Sunday
        var endOfWeek = startOfWeek.AddDays(6); // Saturday

        var streakLogs = await context.StreakLogs
            .Where(s => s.UserId == userId && 
                       s.StreakDate >= startOfWeek && 
                       s.StreakDate <= endOfWeek)
            .Select(s => s.StreakDate)
            .ToListAsync(ct);

        // Build weekly status (Sunday to Saturday)
        var weeklyStatus = new List<DayStreakStatus>();
        var dayNames = new[] { "S", "M", "T", "W", "T", "F", "S" };
        
        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            bool? hasStreak = date > today ? null : streakLogs.Contains(date);
            weeklyStatus.Add(new DayStreakStatus(dayNames[i], hasStreak));
        }

        // Calculate current streak
        var allStreakLogs = await context.StreakLogs
            .Where(s => s.UserId == userId && s.StreakDate <= today)
            .OrderByDescending(s => s.StreakDate)
            .Select(s => s.StreakDate)
            .ToListAsync(ct);

        int currentStreak = 0;
        if (allStreakLogs.Count != 0)
        {
            var lastStreakDate = allStreakLogs.First();
            
            // Streak is valid only if last streak is today or yesterday
            if (lastStreakDate == today || lastStreakDate == today.AddDays(-1))
            {
                currentStreak = 1;
                var checkDate = lastStreakDate.AddDays(-1);

                foreach (var date in allStreakLogs.Skip(1))
                {
                    if (date == checkDate)
                    {
                        currentStreak++;
                        checkDate = checkDate.AddDays(-1);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        // Calculate total points
        var totalPoints = await context.StreakExps
            .Where(s => s.UserId == userId)
            .SumAsync(s => s.Exp, ct);

        var response = new GetUserStreakResponse(
            CurrentStreakDays: currentStreak,
            TotalPoints: totalPoints,
            WeeklyStatus: weeklyStatus);

        await Send.OkAsync(Result.Success(response), ct);
    }
}
