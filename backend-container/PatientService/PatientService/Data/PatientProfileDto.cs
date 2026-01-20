using PatientService.Models;

namespace PatientService.Data;

public record PatientProfileDto(
    Guid Id,
    Guid UserId,
    Guid? PrimaryDoctorId,
    string Name,
    string Email,
    string Phone,
    string Address,
    DateTime? DateOfBirth,
    string Gender,
    string? BloodType,
    List<EmergencyContactDto> EmergencyContacts,
    List<InsuranceDto> Insurance
);

public record EmergencyContactDto(string Name, string? Relation, string? Phone);
public record InsuranceDto(string? Provider, string? PolicyNumber, DateTime? ValidFrom, DateTime? ValidTo);
