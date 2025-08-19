using System;

namespace AppointmentService.Features.Metrics.DTOs;

public class AppointmentMetricsResponse
{
    public int TotalAppointments { get; set; }
    public int AppointmentsThisMonth { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShowAppointments { get; set; }
    public decimal CompletionRate { get; set; }
    public int ActiveDoctorsInPeriod { get; set; }
    public int UniquePatientsInPeriod { get; set; }
    public decimal AverageDurationMinutes { get; set; }
    public decimal AverageRevenuePerAppointment { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsStub { get; set; } = true;
}
