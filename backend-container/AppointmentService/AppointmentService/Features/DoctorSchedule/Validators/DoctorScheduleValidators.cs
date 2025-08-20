using System.ComponentModel.DataAnnotations;

namespace AppointmentService.Features.DoctorSchedule.Validators;

public static class DoctorScheduleValidators
{
    public static bool IsValidStatus(string status)
    {
        return new[] { "scheduled", "completed", "no-show", "cancelled" }.Contains(status.ToLower());
    }
}
