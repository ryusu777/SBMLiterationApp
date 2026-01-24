using FastEndpoints;

namespace PureTCOWebApp.Features.AdminModule;

public class AdminEndpointGroup : Group
{
    public AdminEndpointGroup()
    {
        Configure("users", ep =>
        {
            ep.Roles("admin");
            ep.Description(x => x.WithTags("Admin Module"));
            ep.Tags("Admin Module");
            ep.Group<GlobalApiEndpointGroup>();
        });
    }
}
