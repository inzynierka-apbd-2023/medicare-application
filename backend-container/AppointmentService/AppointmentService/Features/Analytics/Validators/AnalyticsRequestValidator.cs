using System.ComponentModel.DataAnnotations;

namespace AppointmentService.Features.Analytics.Validators;

public static class AnalyticsRequestValidator
{
    public static ValidationResult ValidateDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            if (startDate.Value > endDate.Value)
            {
                return new ValidationResult("Start date cannot be later than end date");
            }

            if (startDate.Value > DateTime.UtcNow)
            {
                return new ValidationResult("Start date cannot be in the future");
            }

            var daysDifference = (endDate.Value - startDate.Value).Days;
            if (daysDifference > 365)
            {
                return new ValidationResult("Date range cannot exceed 365 days");
            }
        }

        return ValidationResult.Success!;
    }

    public static ValidationResult ValidateDoctorId(Guid? doctorId)
    {
        // Guid is strongly typed, so if it's not null, it's valid structure.
        return ValidationResult.Success!;
    }

    public static ValidationResult ValidateSpecialization(string? specialization)
    {
        if (!string.IsNullOrEmpty(specialization))
        {
            if (specialization.Length > 200)
            {
                return new ValidationResult("Specialization name cannot exceed 200 characters");
            }

            if (specialization.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
            {
                return new ValidationResult("Specialization name can only contain letters and spaces");
            }
        }

        return ValidationResult.Success!;
    }

    public static ValidationResult ValidateStatus(string? status)
    {
        if (!string.IsNullOrEmpty(status))
        {
            var validStatuses = new[] { "scheduled", "confirmed", "cancelled", "completed", "no-show" };
            if (!validStatuses.Contains(status.ToLowerInvariant()))
            {
                return new ValidationResult($"Status must be one of: {string.Join(", ", validStatuses)}");
            }
        }

        return ValidationResult.Success!;
    }

    public static List<ValidationResult> ValidateAnalyticsRequest(
        DateTime? startDate, 
        DateTime? endDate, 
        Guid? doctorId, 
        string? specialization, 
        string? status)
    {
        var results = new List<ValidationResult>();

        var dateRangeResult = ValidateDateRange(startDate, endDate);
        if (dateRangeResult != ValidationResult.Success)
            results.Add(dateRangeResult);

        var doctorIdResult = ValidateDoctorId(doctorId);
        if (doctorIdResult != ValidationResult.Success)
            results.Add(doctorIdResult);

        var specializationResult = ValidateSpecialization(specialization);
        if (specializationResult != ValidationResult.Success)
            results.Add(specializationResult);

        var statusResult = ValidateStatus(status);
        if (statusResult != ValidationResult.Success)
            results.Add(statusResult);

        return results;
    }
}
