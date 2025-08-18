using MediatR;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Features.Analytics.DTOs;
using AppointmentService.Features.Analytics.Queries;

namespace AppointmentService.Features.Analytics.Handlers;

public class GetDoctorPerformanceHandler : IRequestHandler<GetDoctorPerformanceQuery, IEnumerable<DoctorPerformanceDto>>
{
    private readonly AppointmentDbContext _context;

    public GetDoctorPerformanceHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DoctorPerformanceDto>> Handle(GetDoctorPerformanceQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        var query = from d in _context.Doctors
                    join u in _context.Users on d.Id equals u.Id
                    join up in _context.UserProfiles on u.Id equals up.User_Id
                    join ds in _context.DoctorSpecializations on d.Id equals ds.Doctor_Id into docSpecs
                    from ds in docSpecs.DefaultIfEmpty()
                    join s in _context.Specializations on ds.Specialization_Id equals s.Id into specs
                    from s in specs.DefaultIfEmpty()
                    where u.Is_Active
                    select new { Doctor = d, Profile = up, Specialization = s != null ? s.Name : "General" };

        if (!string.IsNullOrEmpty(request.DoctorId))
            query = query.Where(x => x.Doctor.Id == request.DoctorId);

        if (!string.IsNullOrEmpty(request.Specialization))
            query = query.Where(x => x.Specialization == request.Specialization);

        var doctors = await query.ToListAsync(cancellationToken);
        var performance = new List<DoctorPerformanceDto>();

        foreach (var doctor in doctors)
        {
            var appointments = await _context.ScheduleAppointments
                .Where(sa => sa.Doctor_User_Id == doctor.Doctor.Id && sa.Day >= startDate && sa.Day <= endDate)
                .ToListAsync(cancellationToken);

            var completed = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Doctor_User_Id == doctor.Doctor.Id && x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "completed")
                .CountAsync(cancellationToken);

            var cancelled = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Doctor_User_Id == doctor.Doctor.Id && x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "cancelled")
                .CountAsync(cancellationToken);

            var noShow = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => x.sa.Doctor_User_Id == doctor.Doctor.Id && x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "no-show")
                .CountAsync(cancellationToken);

            var ratings = await _context.Rates
                .Where(r => r.Doctor_User_Id == doctor.Doctor.Id && r.Rated_At >= startDate && r.Rated_At <= endDate)
                .ToListAsync(cancellationToken);

            var revenue = await _context.AppointmentPayments
                .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                    (ap, sa) => new { ap, sa })
                .Where(x => x.sa.Doctor_User_Id == doctor.Doctor.Id && x.sa.Day >= startDate && x.sa.Day <= endDate && x.ap.Status == "Paid")
                .SumAsync(x => x.ap.Amount, cancellationToken);

            performance.Add(new DoctorPerformanceDto
            {
                Id = doctor.Doctor.Id,
                Name = $"Dr. {doctor.Profile.FirstName} {doctor.Profile.LastName}",
                Specialization = doctor.Specialization,
                TotalAppointments = appointments.Count,
                CompletedAppointments = completed,
                CancelledAppointments = cancelled,
                NoShowAppointments = noShow,
                AverageRating = ratings.Any() ? ratings.Average(r => r.Rate_Value) : 0,
                TotalRatings = ratings.Count,
                Revenue = revenue,
                UtilizationRate = appointments.Count > 0 ? (double)completed / appointments.Count * 100 : 0
            });
        }

        return performance;
    }
}

public class GetSpecializationStatsHandler : IRequestHandler<GetSpecializationStatsQuery, IEnumerable<SpecializationStatsDto>>
{
    private readonly AppointmentDbContext _context;

    public GetSpecializationStatsHandler(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SpecializationStatsDto>> Handle(GetSpecializationStatsQuery request, CancellationToken cancellationToken)
    {
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDate = request.StartDate ?? endDate.AddDays(-30);

        var specializationData = await (from s in _context.Specializations
                                       join ds in _context.DoctorSpecializations on s.Id equals ds.Specialization_Id
                                       join d in _context.Doctors on ds.Doctor_Id equals d.Id
                                       where s.Is_Active
                                       group new { s, ds, d } by s.Name into g
                                       select new
                                       {
                                           SpecializationName = g.Key,
                                           DoctorIds = g.Select(x => x.d.Id).Distinct().ToList()
                                       }).ToListAsync(cancellationToken);

        var stats = new List<SpecializationStatsDto>();

        foreach (var spec in specializationData)
        {
            var appointments = await _context.ScheduleAppointments
                .Where(sa => spec.DoctorIds.Contains(sa.Doctor_User_Id) && sa.Day >= startDate && sa.Day <= endDate)
                .ToListAsync(cancellationToken);

            var completed = await _context.ScheduleAppointments
                .Join(_context.ScheduleAppointmentStatuses, sa => sa.Schedule_Appointment_Status_Id, status => status.Id,
                    (sa, status) => new { sa, status })
                .Where(x => spec.DoctorIds.Contains(x.sa.Doctor_User_Id) && x.sa.Day >= startDate && x.sa.Day <= endDate && x.status.Name == "completed")
                .CountAsync(cancellationToken);

            var patients = await _context.ScheduleAppointments
                .Where(sa => spec.DoctorIds.Contains(sa.Doctor_User_Id) && sa.Day >= startDate && sa.Day <= endDate)
                .Select(sa => sa.Patient_User_Id)
                .Distinct()
                .CountAsync(cancellationToken);

            var revenue = await _context.AppointmentPayments
                .Join(_context.ScheduleAppointments, ap => ap.Schedule_Appointment_Id, sa => sa.Id,
                    (ap, sa) => new { ap, sa })
                .Where(x => spec.DoctorIds.Contains(x.sa.Doctor_User_Id) && x.sa.Day >= startDate && x.sa.Day <= endDate && x.ap.Status == "Paid")
                .SumAsync(x => x.ap.Amount, cancellationToken);

            var avgRating = await _context.Rates
                .Where(r => spec.DoctorIds.Contains(r.Doctor_User_Id) && r.Rated_At >= startDate && r.Rated_At <= endDate)
                .AverageAsync(r => (double?)r.Rate_Value, cancellationToken) ?? 0;

            stats.Add(new SpecializationStatsDto
            {
                Specialization = spec.SpecializationName,
                TotalAppointments = appointments.Count,
                TotalPatients = patients,
                TotalDoctors = spec.DoctorIds.Count,
                AverageAppointmentDuration = appointments.Any() ? appointments.Average(a => a.Duration_Minutes) : 0,
                Revenue = revenue,
                CompletionRate = appointments.Count > 0 ? (double)completed / appointments.Count * 100 : 0,
                AverageRating = avgRating
            });
        }

        return stats;
    }
}

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
            .Where(sa => string.IsNullOrEmpty(request.DoctorId) || sa.Doctor_User_Id == request.DoctorId)
            .ToListAsync(cancellationToken);

        var timeSlots = new List<TimeSlotDataDto>();
        var weeklyData = new List<DayDataDto>();

        // Group by hour for time slot analysis
        var hourlyData = appointments
            .GroupBy(a => a.Day.Hour)
            .Where(g => g.Key >= 8 && g.Key <= 18) // Business hours
            .OrderBy(g => g.Key);

        foreach (var hourGroup in hourlyData)
        {
            var hour = hourGroup.Key;
            var hourAppointments = hourGroup.ToList();
            
            var timeSlotData = new TimeSlotDataDto
            {
                Hour = hour,
                TimeSlot = $"{hour:D2}:00-{(hour + 1):D2}:00",
                TotalAppointments = hourAppointments.Count
            };

            // Count by day of week
            timeSlotData.Monday = hourAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Monday);
            timeSlotData.Tuesday = hourAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Tuesday);
            timeSlotData.Wednesday = hourAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Wednesday);
            timeSlotData.Thursday = hourAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Thursday);
            timeSlotData.Friday = hourAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Friday);
            timeSlotData.Saturday = hourAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Saturday);
            timeSlotData.Sunday = hourAppointments.Count(a => a.Day.DayOfWeek == DayOfWeek.Sunday);

            timeSlots.Add(timeSlotData);
        }

        // Group by day of week for weekly analysis
        var weekDays = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        
        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DayOfWeek)((i + 1) % 7); // Adjust for Sunday = 0
            var dayAppointments = appointments.Where(a => a.Day.DayOfWeek == dayOfWeek).ToList();
            
            var peakHour = dayAppointments
                .GroupBy(a => a.Day.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            weeklyData.Add(new DayDataDto
            {
                Day = weekDays[i],
                TotalAppointments = dayAppointments.Count,
                PeakHour = peakHour != null ? $"{peakHour.Key:D2}:00-{(peakHour.Key + 1):D2}:00" : "-",
                UtilizationRate = dayAppointments.Count > 0 ? Math.Min(100, dayAppointments.Count * 10) : 0 // Simple calculation
            });
        }

        return new TimeSlotAnalysisDto
        {
            TimeSlots = timeSlots,
            WeeklyData = weeklyData
        };
    }
}
