using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

namespace PureTCOWebApp.Features.StreakModule.Domain.Events;

public class StreakBonusEventHandler : IDomainEventHandler<ReadingReportCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public StreakBonusEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ReadingReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var userId = domainEvent.Report.UserId;
        var today = DateOnly.FromDateTime(domainEvent.Report.CreateTime);
        
        // Check if user has 7 consecutive days
        if (!await HasSevenConsecutiveDays(userId, today, cancellationToken))
            return;
        
        // Check if already rewarded for this streak period
        var streakDateFrom = today.AddDays(-6);
        var exists = await _dbContext.StreakExps
            .AnyAsync(s => s.UserId == userId && 
                          s.StreakDateFrom == streakDateFrom &&
                          s.Duration == 7, cancellationToken);
        
        if (exists)
            return;
        
        // Create StreakExp for 7-day bonus
        var streakBonus = StreakExp.Create(
            userId: userId,
            streakDateFrom: streakDateFrom,
            duration: 7,
            exp: 70
        );
        
        await _dbContext.StreakExps.AddAsync(streakBonus, cancellationToken);
    }

    private async Task<bool> HasSevenConsecutiveDays(int userId, DateOnly endDate, CancellationToken cancellationToken)
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => endDate.AddDays(-i))
            .OrderBy(d => d)
            .ToList();
        
        var streakLogs = await _dbContext.StreakLogs
            .Where(s => s.UserId == userId && last7Days.Contains(s.StreakDate))
            .Select(s => s.StreakDate)
            .ToListAsync(cancellationToken);
        
        if (streakLogs.Count != 7)
            return false;
        
        var sorted = streakLogs.OrderBy(d => d).ToList();
        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i].DayNumber != sorted[i - 1].DayNumber + 1)
                return false;
        }
        
        return true;
    }
}
