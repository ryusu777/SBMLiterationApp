using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.AdminModule.Endpoints;

public record DisableUserRequest(int UserId);

public class DisableUserEndpoint(UserManager<User> userManager)
    : Endpoint<DisableUserRequest>
{
    public override void Configure()
    {
        Get("{UserId}/disable");
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
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description))));
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
