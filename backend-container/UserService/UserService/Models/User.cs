using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("User")]
public class User
{
    [Key]
    [Column("Id")]
    public string Id { get; set; } = string.Empty;

    [Column("Role_Id")]
    public string? RoleId { get; set; }

    [Column("Schedule_Id")]
    public string? ScheduleId { get; set; }

    [Column("Created_At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("Updated_At")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("Is_Active")]
    public bool IsActive { get; set; } = true;

    // Authentication fields - we'll add these to the existing table
    [Column("Username")]
    [MaxLength(50)]
    public string? Username { get; set; }

    [Column("PasswordHash")]
    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }

    public virtual UserProfile? Profile { get; set; }
}
