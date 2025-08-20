using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Services;

namespace PractitionerService.Features.StaffManagement.Commands
{
    public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, ApiResponse<StaffMemberDto>>
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<UpdateStaffCommandHandler> _logger;
        
        public UpdateStaffCommandHandler(IStaffService staffService, ILogger<UpdateStaffCommandHandler> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public async Task<ApiResponse<StaffMemberDto>> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate updates if provided
                if (request.Request.Profile?.Email != null && !IsValidEmail(request.Request.Profile.Email))
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Validation failed",
                        Errors = new List<string> { "Invalid email format" }
                    };
                }

                if (!string.IsNullOrWhiteSpace(request.Request.Role) &&
                    request.Request.Role != "Doctor" && request.Request.Role != "Receptionist")
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Validation failed",
                        Errors = new List<string> { "Role must be either 'Doctor' or 'Receptionist'" }
                    };
                }

                var updatedStaff = await _staffService.UpdateStaffMemberAsync(request.Request.Id, request.Request, cancellationToken);

                if (updatedStaff == null)
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Staff member not found",
                        Errors = new List<string> { $"No staff member found with ID: {request.Request.Id}" }
                    };
                }

                _logger.LogInformation("Updated staff member with ID {StaffId}", updatedStaff.Id);

                return new ApiResponse<StaffMemberDto>
                {
                    Success = true,
                    Data = updatedStaff,
                    Message = "Staff member updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update staff member with ID {StaffId}", request.Request.Id);
                return new ApiResponse<StaffMemberDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to update staff member",
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
