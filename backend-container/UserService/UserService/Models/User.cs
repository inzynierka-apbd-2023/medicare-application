using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("User")]
public class User
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("Role_Id")]
    public Guid? RoleId { get; set; }

    [Column("Schedule_Id")]
    public Guid? ScheduleId { get; set; }

    [Column("Created_At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("Updated_At")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("Is_Active")]
    public bool IsActive { get; set; } = true;

    [Column("Username")]
    [MaxLength(50)]
    public string? Username { get; set; }

    [Column("PasswordHash")]
    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }

    public virtual UserProfile? Profile { get; set; }
}
