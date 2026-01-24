namespace AppointmentService.Features.Analytics.DTOs;

public class AppointmentAnalyticsResponse
{
    public IEnumerable<AppointmentMetricDto> Metrics { get; set; } = new List<AppointmentMetricDto>();
    public IEnumerable<TrendDataDto> Trends { get; set; } = new List<TrendDataDto>();
    public IEnumerable<DoctorPerformanceDto> DoctorPerformance { get; set; } = new List<DoctorPerformanceDto>();
    public IEnumerable<SpecializationStatsDto> SpecializationStats { get; set; } = new List<SpecializationStatsDto>();
    public TimeSlotAnalysisDto TimeAnalysis { get; set; } = new();
}

public class AppointmentMetricDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Value { get; set; }
    public double Change { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class TrendDataDto
{
    public string Date { get; set; } = string.Empty;
    public int Appointments { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public decimal Revenue { get; set; }
}

public class DoctorPerformanceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShowAppointments { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public decimal Revenue { get; set; }
    public double UtilizationRate { get; set; }
}

public class SpecializationStatsDto
{
    public string Specialization { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public double AverageAppointmentDuration { get; set; }
    public decimal Revenue { get; set; }
    public double CompletionRate { get; set; }
    public double AverageRating { get; set; }
}

public class TimeSlotDataDto
{
    public int Hour { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public int Monday { get; set; }
    public int Tuesday { get; set; }
    public int Wednesday { get; set; }
    public int Thursday { get; set; }
    public int Friday { get; set; }
    public int Saturday { get; set; }
    public int Sunday { get; set; }
    public int TotalAppointments { get; set; }
    public decimal AverageRevenue { get; set; }
    public double CompletionRate { get; set; }
}

public class DayDataDto
{
    public string Day { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public string PeakHour { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public double UtilizationRate { get; set; }
}

public class TimeSlotAnalysisDto
{
    public IEnumerable<TimeSlotDataDto> TimeSlots { get; set; } = new List<TimeSlotDataDto>();
    public IEnumerable<DayDataDto> WeeklyData { get; set; } = new List<DayDataDto>();
}

public class DoctorProfileDto
{
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string SpecializationNames { get; set; } = string.Empty;
}

public class AppointmentPaymentDto
{
    public Guid AppointmentId { get; set; }
    public long AmountCents { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RatingDto
{
    public Guid AppointmentId { get; set; }
    public byte RateValue { get; set; }
}

