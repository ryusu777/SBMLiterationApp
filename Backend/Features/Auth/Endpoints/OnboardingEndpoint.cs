using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.Auth.Endpoints;

public class OnboardingRequest
{
    public string Nim { get; set; } = string.Empty;
    public string ProgramStudy { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public int? GenerationYear { get; set; }
}


public class OnboardingEndpoint : Endpoint<OnboardingRequest>
{
    private readonly UserManager<User> _userManager;

    public OnboardingEndpoint(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public override void Configure()
    {
        Put("/api/auth/onboarding");
    }

    public override async Task HandleAsync(OnboardingRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await _userManager.FindByIdAsync(userIdClaim);

        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        user.UpdateOnboardingInfo(req.Nim, req.ProgramStudy, req.Faculty, req.GenerationYear);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            AddError(string.Join(", ", result.Errors.Select(e => e.Description)));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
