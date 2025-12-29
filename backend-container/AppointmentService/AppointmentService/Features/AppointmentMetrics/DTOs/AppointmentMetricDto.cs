namespace AppointmentService.Features.AppointmentMetrics.DTOs;

public class AppointmentMetricDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Value { get; set; }
    public double Change { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
