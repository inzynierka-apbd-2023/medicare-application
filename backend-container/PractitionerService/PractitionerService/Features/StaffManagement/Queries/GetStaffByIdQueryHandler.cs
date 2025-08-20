using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Services;

namespace PractitionerService.Features.StaffManagement.Queries
{
    public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, ApiResponse<StaffMemberDto>>
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<GetStaffByIdQueryHandler> _logger;
        
        public GetStaffByIdQueryHandler(IStaffService staffService, ILogger<GetStaffByIdQueryHandler> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public async Task<ApiResponse<StaffMemberDto>> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var staffMember = await _staffService.GetStaffMemberByIdAsync(request.Id, cancellationToken);

                if (staffMember == null)
                {
                    return new ApiResponse<StaffMemberDto>
                    {
                        Success = false,
                        Data = null,
                        Message = "Staff member not found",
                        Errors = new List<string> { $"No staff member found with ID: {request.Id}" }
                    };
                }

                return new ApiResponse<StaffMemberDto>
                {
                    Success = true,
                    Data = staffMember,
                    Message = "Staff member retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve staff member with ID {StaffId}", request.Id);
                return new ApiResponse<StaffMemberDto>
                {
                    Success = false,
                    Data = null,
                    Message = "Failed to retrieve staff member",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
