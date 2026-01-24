using FastEndpoints;

namespace PureTCOWebApp.Features.StreakModule.Endpoints;

public class StreakEndpointGroup : Group
{
    public StreakEndpointGroup()
    {
        Configure("streaks", ep =>
        {
            ep.Description(x => x.WithTags("Streaks"));
            ep.Tags("Streaks");
            ep.Group<GlobalApiEndpointGroup>();
        });
    }
}
