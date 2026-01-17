namespace PureTCOWebApp.Features.Auth.Domain;

public class RefreshToken
{
    #pragma warning disable 
    public RefreshToken()
    {
    }
    #pragma warning restore
    
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Jti { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}