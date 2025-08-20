using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Services;

namespace PractitionerService.Features.StaffManagement.Queries
{
    public class GetStaffByRoleQueryHandler : IRequestHandler<GetStaffByRoleQuery, ApiResponse<List<StaffMemberDto>>>
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<GetStaffByRoleQueryHandler> _logger;
        
        public GetStaffByRoleQueryHandler(IStaffService staffService, ILogger<GetStaffByRoleQueryHandler> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<StaffMemberDto>>> Handle(GetStaffByRoleQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var staffMembers = await _staffService.GetStaffMembersByRoleAsync(request.Role, cancellationToken);

                return new ApiResponse<List<StaffMemberDto>>
                {
                    Success = true,
                    Data = staffMembers,
                    Message = $"Retrieved {staffMembers.Count} {request.Role} staff members"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve {Role} staff members", request.Role);
                return new ApiResponse<List<StaffMemberDto>>
                {
                    Success = false,
                    Data = new List<StaffMemberDto>(),
                    Message = $"Failed to retrieve {request.Role} staff members",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
