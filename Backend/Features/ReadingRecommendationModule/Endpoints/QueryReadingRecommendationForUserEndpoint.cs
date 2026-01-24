using FastEndpoints;
using PureTCOWebApp.Core.Paging;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingRecommendationModule.Domain;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Endpoints;

public record QueryReadingRecommendationForUserRequest(
    string? Title = null,
    string? Category = null,
    string? Author = null
) : PagingQuery;

public class QueryReadingRecommendationForUserEndpoint(ApplicationDbContext dbContext)
    : Endpoint<QueryReadingRecommendationForUserRequest, PagingResult<ReadingRecommendation>>
{
    public override void Configure()
    {
        Get("participant");
        Group<ReadingRecommendationEndpointGroup>();
    }

    public override async Task HandleAsync(QueryReadingRecommendationForUserRequest req, CancellationToken ct)
    {
        var currentUserId = int.Parse(User.FindFirst("sub")!.Value);

        // Get all ISBNs from user's library (both Books and JournalPapers use ISBN field)
        var userBookIsbns = dbContext.Books
            .Where(b => b.UserId == currentUserId)
            .Select(b => b.ISBN);
        
        var userJournalIsbns = dbContext.JournalPapers
            .Where(j => j.UserId == currentUserId)
            .Select(j => j.ResourceLink);

        var query = dbContext
            .ReadingRecommendations
            .Where(r => !userBookIsbns.Contains(r.ISBN) && !userJournalIsbns.Contains(r.ResourceLink));

        var result = await PagingService.PaginateQueryAsync(query, req, dbContext, ct);

        await Send.OkAsync(result, ct);
    }
}
