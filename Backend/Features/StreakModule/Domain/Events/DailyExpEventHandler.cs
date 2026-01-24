using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

namespace PureTCOWebApp.Features.StreakModule.Domain.Events;

public class DailyExpEventHandler : IDomainEventHandler<ReadingReportCreatedEvent>
{
    private readonly ApplicationDbContext _dbContext;

    public DailyExpEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ReadingReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var report = domainEvent.Report;
        var today = DateOnly.FromDateTime(report.CreateTime);
        
        // Get previous report for same book
        var previousReport = await _dbContext.ReadingReports
            .Where(r => r.UserId == report.UserId &&
                        r.ReadingResourceId == report.ReadingResourceId &&
                        r.Id != report.Id &&
                        r.CreateTime < report.CreateTime)
            .OrderByDescending(r => r.CreateTime)
            .FirstOrDefaultAsync(cancellationToken);
        
        // Calculate pages read
        int previousPage = previousReport?.CurrentPage ?? 0;
        int pagesRead = report.CurrentPage - previousPage;
        
        // Create StreakExp for daily pages (only if positive)
        if (pagesRead > 0)
        {
            var dailyExp = StreakExp.Create(
                userId: report.UserId,
                streakDateFrom: today,
                duration: 1,
                exp: pagesRead
            );
            
            await _dbContext.StreakExps.AddAsync(dailyExp, cancellationToken);
        }
    }
}
