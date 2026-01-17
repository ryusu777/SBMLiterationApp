using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.Auth.Endpoints;

public class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, RefreshTokenResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<User> _userManager;

    public RefreshTokenEndpoint(IJwtTokenService jwtTokenService, UserManager<User> userManager)
    {
        _jwtTokenService = jwtTokenService;
        _userManager = userManager;
    }

    public override void Configure()
    {
        Post("/api/auth/refresh");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.RefreshToken) || string.IsNullOrEmpty(req.AccessToken))
        {
            await Send.ResultAsync(TypedResults.BadRequest(new RefreshTokenResponse
            {
                Success = false,
                Message = "Both access token and refresh token are required"
            }));
            return;
        }

        try
        {
            // Validate the refresh token
            var refreshToken = await _jwtTokenService.ValidateRefreshTokenAsync(req.RefreshToken);

            if (refreshToken == null)
            {
                await Send.ResultAsync(TypedResults.BadRequest(new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                }));
                return;
            }

            // Get principal from expired access token
            ClaimsPrincipal? principal;
            try
            {
                principal = _jwtTokenService.GetPrincipalFromExpiredToken(req.AccessToken);
            }
            catch (Exception)
            {
                await Send.ResultAsync(TypedResults.BadRequest(new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid access token"
                }));
                return;
            }

            if (principal == null)
            {
                await Send.ResultAsync(TypedResults.BadRequest(new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid access token"
                }));
                return;
            }

            // Extract JTI from access token
            var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            
            // Verify JTI matches the refresh token
            if (jti != refreshToken.Jti)
            {
                await Send.ResultAsync(TypedResults.BadRequest(new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Token mismatch"
                }));
                return;
            }

            // Get the user
            var user = refreshToken.User;

            if (user == null)
            {
                await Send.ResultAsync(TypedResults.NotFound(new RefreshTokenResponse
                {
                    Success = false,
                    Message = "User not found"
                }));
                return;
            }

            // Revoke the old refresh token (delete it)
            await _jwtTokenService.RevokeRefreshTokenAsync(req.RefreshToken);

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate new tokens
            var (newAccessToken, newRefreshToken) = await _jwtTokenService.GenerateTokensAsync(user, roles);

            // Return new tokens
            await Send.OkAsync(new RefreshTokenResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during token refresh");
            await Send.ErrorsAsync(500, ct);
        }
    }
}

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
