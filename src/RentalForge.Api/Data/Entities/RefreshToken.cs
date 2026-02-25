namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a single-use refresh token with family-based rotation tracking.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; init; }
    public string Token { get; init; } = null!;
    public string Family { get; init; } = null!;
    public string UserId { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
    public bool IsUsed { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// PostgreSQL xmin system column used as an optimistic concurrency token.
    /// </summary>
    public uint xmin { get; set; }

    public ApplicationUser User { get; init; } = null!;
}
