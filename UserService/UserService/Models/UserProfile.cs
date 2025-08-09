using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("User_Profile")]
public class UserProfile
{
    [Key]
    [Column("User_Id")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column("FirstName")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Column("LastName")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Column("Email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Column("Phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("DateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [Column("Gender")]
    [MaxLength(20)]
    public string? Gender { get; set; }

    [Column("Avatar_Url")]
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [Column("Address_Line1")]
    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    [Column("Address_Line2")]
    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [Column("City")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Column("State")]
    [MaxLength(100)]
    public string? State { get; set; }

    [Column("ZipCode")]
    [MaxLength(20)]
    public string? ZipCode { get; set; }

    [Column("Country")]
    [MaxLength(100)]
    public string? Country { get; set; }

    [Column("Created_At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("Updated_At")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
