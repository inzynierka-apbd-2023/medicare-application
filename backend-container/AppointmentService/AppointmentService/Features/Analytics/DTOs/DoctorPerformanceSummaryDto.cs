namespace AppointmentService.Features.Analytics.DTOs;

public class DoctorPerformanceSummaryDto
{
    public int TotalDoctors { get; set; }
    public decimal AverageAppointmentsPerDoctor { get; set; }
    public string TopRatedDoctor { get; set; } = "N/A";
    public decimal DoctorAverageRating { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
