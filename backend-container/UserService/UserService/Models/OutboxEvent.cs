using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models;

[Table("Outbox_Event", Schema = "user")]
public class OutboxEvent
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Type { get; set; } = default!;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public string PayloadJson { get; set; } = default!;

    public DateTime? PublishedAt { get; set; }
}
