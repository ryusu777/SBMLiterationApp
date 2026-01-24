using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

namespace PureTCOWebApp.Features.StreakModule.Domain.Events;

public class ReadingReportCreatedEventHandler : IDomainEventHandler<ReadingReportCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public ReadingReportCreatedEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ReadingReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var userId = domainEvent.Report.UserId;
        var streakDate = DateOnly.FromDateTime(domainEvent.Report.CreateTime);

        var exists = await _dbContext.StreakLogs
            .AnyAsync(s => s.UserId == userId && s.StreakDate == streakDate, cancellationToken);

        if (!exists)
        {
            var streakLog = StreakLog.Create(userId, streakDate);
            await _dbContext.StreakLogs.AddAsync(streakLog, cancellationToken);
        }
    }
}
