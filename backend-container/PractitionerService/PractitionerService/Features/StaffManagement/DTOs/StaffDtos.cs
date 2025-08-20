using System.ComponentModel.DataAnnotations;

namespace PractitionerService.Features.StaffManagement.DTOs;

public class ProfileDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = default!;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = default!;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = default!;
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Gender { get; set; } = default!;
    
    [Required]
    [StringLength(255)]
    public string AddressLine1 { get; set; } = default!;
    
    [StringLength(255)]
    public string? AddressLine2 { get; set; }
    
    [Required]
    [StringLength(100)]
    public string City { get; set; } = default!;
    
    [Required]
    [StringLength(100)]
    public string State { get; set; } = default!;
    
    [Required]
    [StringLength(20)]
    public string ZipCode { get; set; } = default!;
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = default!;
}

public class SpecializationDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ServiceName { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime? CertifiedDate { get; set; }
}

public class ServiceDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class StaffMemberDto
{
    public string Id { get; set; } = default!;
    public string Role { get; set; } = default!;
    public ProfileDto Profile { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DoctorDto : StaffMemberDto
{
    [Required]
    [StringLength(50)]
    public string LicenseNumber { get; set; } = default!;
    
    [Range(0, 50)]
    public int YearsExperience { get; set; }
    
    [StringLength(1000)]
    public string? Biography { get; set; }
    
    [StringLength(255)]
    public string? OfficeAddress { get; set; }
    
    public List<SpecializationDto> Specializations { get; set; } = new();
}

public class ReceptionistDto : StaffMemberDto
{
    [Required]
    [StringLength(255)]
    public string Department { get; set; } = default!;
}

public class CreateStaffRequest
{
    [Required]
    [StringLength(20)]
    public string Role { get; set; } = default!; // "Doctor" or "Receptionist"
    
    [Required]
    public ProfileDto Profile { get; set; } = default!;
    
    // Doctor-specific fields
    [StringLength(50)]
    public string? LicenseNumber { get; set; }
    
    [Range(0, 50)]
    public int? YearsExperience { get; set; }
    
    [StringLength(1000)]
    public string? Biography { get; set; }
    
    [StringLength(255)]
    public string? OfficeAddress { get; set; }
    
    public List<string>? Specializations { get; set; }
    
    // Receptionist-specific fields
    [StringLength(255)]
    public string? Department { get; set; }
}

public class UpdateStaffRequest
{
    [Required]
    public string Id { get; set; } = default!;
    
    [Required]
    [StringLength(20)]
    public string Role { get; set; } = default!;
    
    public ProfileDto? Profile { get; set; }
    
    // Doctor-specific fields
    [StringLength(50)]
    public string? LicenseNumber { get; set; }
    
    [Range(0, 50)]
    public int? YearsExperience { get; set; }
    
    [StringLength(1000)]
    public string? Biography { get; set; }
    
    [StringLength(255)]
    public string? OfficeAddress { get; set; }
    
    public List<string>? Specializations { get; set; }
    
    // Receptionist-specific fields
    [StringLength(255)]
    public string? Department { get; set; }
}

public class StaffSearchRequest
{
    public string? Role { get; set; } // "Doctor" or "Receptionist"
    public string? SearchQuery { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? SpecializationIds { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
