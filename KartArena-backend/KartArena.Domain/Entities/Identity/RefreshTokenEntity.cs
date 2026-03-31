using KartArena.Domain.Common;

namespace KartArena.Domain.Entities.Identity;

public sealed class RefreshTokenEntity : BaseEntity
{
    public string TokenHash { get; set; } = default!; // Store the HASH, not plain token
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public string? Fingerprint { get; set; } // Optional: e.g., UA/IP hash
    public DateTime? RevokedAtUtc { get; set; }

    public int UserId { get; set; }

    public UserEntity User { get; set; } = default!;
}
