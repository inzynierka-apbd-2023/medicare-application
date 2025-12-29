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

        var appointments = await _context.ScheduleAppointments
            .Where(sa => sa.Day >= startDate && sa.Day <= endDate)
            .Where(sa => !request.DoctorId.HasValue || sa.Doctor_User_Id == request.DoctorId)
            .ToListAsync(cancellationToken);

        // Create time slot data for hourly analysis
        var timeSlots = new List<TimeSlotDataDto>();
        
        for (int hour = 8; hour <= 17; hour++) // Business hours 8 AM to 5 PM
        {
            var hourlyAppointments = appointments.Where(a => a.Day.Hour == hour).ToList();
            
            var timeSlot = new TimeSlotDataDto
            {
                Hour = hour,
                TimeSlot = $"{hour:00}:00 - {hour + 1:00}:00",
                Monday = hourlyAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Monday),
                Tuesday = hourlyAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Tuesday),
                Wednesday = hourlyAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Wednesday),
                Thursday = hourlyAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Thursday),
                Friday = hourlyAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Friday),
                Saturday = hourlyAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Saturday),
                Sunday = hourlyAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Sunday),
                TotalAppointments = hourlyAppointments.Count
            };

            if (timeSlot.TotalAppointments > 0)
            {
                // Calculate average revenue for this time slot
                var revenue = await _context.AppointmentPayments
                    .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                        (ap, sa) => new { ap, sa })
                    .Where(x => hourlyAppointments.Select(h => h.Id).Contains(x.sa.Id) && x.ap.Status == "Paid")
                    .AverageAsync(x => (decimal?)x.ap.Amount, cancellationToken) ?? 0m;

                timeSlot.AverageRevenue = revenue;

                // Calculate completion rate for this time slot
                var completed = await _context.ScheduleAppointments
                    .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                        (sa, status) => new { sa, status })
                    .Where(x => hourlyAppointments.Select(h => h.Id).Contains(x.sa.Id) && x.status.Name == "Completed")
                    .CountAsync(cancellationToken);

                timeSlot.CompletionRate = ((double)completed / timeSlot.TotalAppointments) * 100;
            }

            timeSlots.Add(timeSlot);
        }

        // Create weekly data analysis
        var weeklyData = new List<DayDataDto>();
        var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        
        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DayOfWeek)((i + 1) % 7); // Monday = 1, Sunday = 0
            var dayAppointments = appointments.Where(a => a.Day.DayOfWeek == dayOfWeek).ToList();
            
            var peakHour = dayAppointments
                .GroupBy(a => a.Day.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? 9;

            var dayRevenue = await _context.AppointmentPayments
                .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                    (ap, sa) => new { ap, sa })
                .Where(x => dayAppointments.Select(d => d.Id).Contains(x.sa.Id) && x.ap.Status == "Paid")
                .SumAsync(x => x.ap.Amount ?? 0m, cancellationToken);

            weeklyData.Add(new DayDataDto
            {
                Day = dayNames[i],
                TotalAppointments = dayAppointments.Count,
                PeakHour = $"{peakHour:00}:00",
                Revenue = dayRevenue,
                UtilizationRate = dayAppointments.Count > 0 ? (dayAppointments.Count / 10.0) * 100 : 0 // Assuming 10 slots per day
            });
        }

        return new TimeSlotAnalysisDto
        {
            TimeSlots = timeSlots,
            WeeklyData = weeklyData
        };
    }
}
