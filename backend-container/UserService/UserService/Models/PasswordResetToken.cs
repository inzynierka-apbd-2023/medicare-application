using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("PasswordResetToken")]
public class PasswordResetToken
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("User_Id")]
    public Guid UserId { get; set; }

    [Column("Token_Hash")]
    [MaxLength(100)]
    public string TokenHash { get; set; } = default!;

    [Column("Expires_At")]
    public DateTime ExpiresAt { get; set; }

    [Column("Created_At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("Used_At")]
    public DateTime? UsedAt { get; set; }

    // Navigation
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
