namespace AppointmentService.Features.Scheduler.DTOs;

public class SchedulerStatsResponse
{
    public int TotalAppointments { get; set; }
    public int TodaysAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
}
