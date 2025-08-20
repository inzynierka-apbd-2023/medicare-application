namespace AppointmentService.Features.DoctorDashboard.DTOs;

public class DoctorQuickStatsDto
{
    public string Label { get; set; } = default!;
    public int Value { get; set; }
    public string? Change { get; set; }
    public string? Trend { get; set; }
}

public class DoctorQuickStatsResponse
{
    public List<DoctorQuickStatsDto> Stats { get; set; } = new();
}
