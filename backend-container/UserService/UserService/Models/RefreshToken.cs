using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("Refresh_Token", Schema = "user")]
public class RefreshToken
{
    [Key]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("User_Id")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    [Column("Token_Hash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("Expires_At")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    [Column("Created_At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("Revoked_At")]
    public DateTime? RevokedAt { get; set; } = null;

    [MaxLength(128)]
    [Column("Replaced_By_Hash")]
    public string? ReplacedByTokenHash { get; set; } = null;

    [MaxLength(45)]
    [Column("Created_By_Ip")]
    public string? CreatedByIp { get; set; } = null;

    [MaxLength(45)]
    [Column("Revoked_By_Ip")]
    public string? RevokedByIp { get; set; } = null;

    [MaxLength(512)]
    [Column("User_Agent")]
    public string? UserAgent { get; set; } = null;

    [NotMapped]
    public bool IsActive => RevokedAt == null && DateTime.UtcNow <= ExpiresAt;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
