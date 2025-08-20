using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Services;

namespace PractitionerService.Features.StaffManagement.Commands
{
    public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, ApiResponse<StaffMemberDto>>
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<CreateStaffCommandHandler> _logger;
        
        public CreateStaffCommandHandler(IStaffService staffService, ILogger<CreateStaffCommandHandler> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public async Task<ApiResponse<StaffMemberDto>> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Request.Profile.FirstName))
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Validation failed",
                        Errors = new List<string> { "First name is required" }
                    };
                }
                
                if (string.IsNullOrWhiteSpace(request.Request.Profile.LastName))
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Validation failed",
                        Errors = new List<string> { "Last name is required" }
                    };
                }
                
                if (string.IsNullOrWhiteSpace(request.Request.Profile.Email))
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Validation failed",
                        Errors = new List<string> { "Email is required" }
                    };
                }

                // Validate email format
                if (!IsValidEmail(request.Request.Profile.Email))
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Validation failed",
                        Errors = new List<string> { "Invalid email format" }
                    };
                }

                // Validate role
                if (request.Request.Role != "Doctor" && request.Request.Role != "Receptionist")
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Validation failed",
                        Errors = new List<string> { "Role must be either 'Doctor' or 'Receptionist'" }
                    };
                }

                // Role-specific validations
                if (request.Request.Role == "Doctor")
                {
                    if (string.IsNullOrWhiteSpace(request.Request.LicenseNumber))
                    {
                        return new ApiResponse<StaffMemberDto>
                        {
                            Success = false,
                            Data = null,
                            Message = "Validation failed",
                            Errors = new List<string> { "License number is required for doctors" }
                        };
                    }
                    
                    if (!request.Request.YearsExperience.HasValue || request.Request.YearsExperience < 0)
                    {
                        return new ApiResponse<StaffMemberDto>
                        {
                            Success = false,
                            Data = null,
                            Message = "Validation failed",
                            Errors = new List<string> { "Years of experience is required for doctors and must be non-negative" }
                        };
                    }
                }

                if (request.Request.Role == "Receptionist")
                {
                    if (string.IsNullOrWhiteSpace(request.Request.Department))
                    {
                        return new ApiResponse<StaffMemberDto>
                        {
                            Success = false,
                            Data = null,
                            Message = "Validation failed",
                            Errors = new List<string> { "Department is required for receptionists" }
                        };
                    }
                }

                var newStaff = await _staffService.CreateStaffMemberAsync(request.Request, cancellationToken);

                if (newStaff == null)
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Failed to create staff member",
                        Errors = new List<string> { "Staff member creation failed" }
                    };
                }

                _logger.LogInformation("Created new staff member with ID {StaffId}", newStaff.Id);

                return new ApiResponse<StaffMemberDto>
                {
                    Success = true,
                    Data = newStaff,
                    Message = "Staff member created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create staff member");
                return new ApiResponse<StaffMemberDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to create staff member",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
