using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetTimeSlotAnalysisHandler : IRequestHandler<GetTimeSlotAnalysisQuery, TimeSlotAnalysisDto>
{
    private readonly AppointmentDbContext _context;

    public GetTimeSlotAnalysisHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<TimeSlotAnalysisDto> Handle(GetTimeSlotAnalysisQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        // Use the actual Appointments table
        var query = _context.Appointments
            .Where(a => a.ScheduledAt >= startDate && a.ScheduledAt <= endDate);

        if (request.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == request.DoctorId);

        var appointments = await query.ToListAsync(cancellationToken);

        // Build time slot analysis
        var timeSlots = new List<TimeSlotDataDto>();
        for (int hour = 8; hour < 18; hour++) // 8 AM to 6 PM
        {
            var hourAppointments = appointments.Where(a => a.ScheduledAt.Hour == hour).ToList();
            
            timeSlots.Add(new TimeSlotDataDto
            {
                Hour = hour,
                TimeSlot = $"{hour:D2}:00 - {(hour + 1):D2}:00",
                Monday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Monday),
                Tuesday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Tuesday),
                Wednesday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Wednesday),
                Thursday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Thursday),
                Friday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Friday),
                Saturday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Saturday),
                Sunday = hourAppointments.Count(a => a.ScheduledAt.DayOfWeek == DayOfWeek.Sunday),
                TotalAppointments = hourAppointments.Count,
                AverageRevenue = 0, // Not available
                CompletionRate = hourAppointments.Count > 0 
                    ? (double)hourAppointments.Count(a => a.Status == "Completed") / hourAppointments.Count * 100 
                    : 0
            });
        }

        // Build weekly data
        var weeklyData = new List<DayDataDto>();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var dayAppointments = appointments.Where(a => a.ScheduledAt.DayOfWeek == day).ToList();
            var peakHour = dayAppointments
                .GroupBy(a => a.ScheduledAt.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? 9;

            weeklyData.Add(new DayDataDto
            {
                Day = day.ToString(),
                TotalAppointments = dayAppointments.Count,
                PeakHour = $"{peakHour:D2}:00",
                Revenue = 0, // Not available
                UtilizationRate = 0 // Would need schedule data to calculate properly
            });
        }

        return new TimeSlotAnalysisDto
        {
            TimeSlots = timeSlots,
            WeeklyData = weeklyData
        };
    }
}
