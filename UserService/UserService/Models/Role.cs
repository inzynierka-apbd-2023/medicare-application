using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("Role")]
public class Role
{
    [Key]
    [Column("Id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [Column("Name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("Description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation property
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
