using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.AdminModule.Endpoints;

public record DisableUserRequest(int UserId);

public class DisableUserEndpoint(UserManager<User> userManager)
    : Endpoint<DisableUserRequest>
{
    public override void Configure()
    {
        Post("{UserId}/disable");
        Group<AdminEndpointGroup>();
    }

    public override async Task HandleAsync(DisableUserRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.UserId.ToString());
        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!result.Succeeded)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(ct, ct);
    }
}
