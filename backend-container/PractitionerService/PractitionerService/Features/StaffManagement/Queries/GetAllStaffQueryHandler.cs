using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Services;

namespace PractitionerService.Features.StaffManagement.Queries
{
    public class GetAllStaffQueryHandler : IRequestHandler<GetAllStaffQuery, ApiResponse<List<StaffMemberDto>>>
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<GetAllStaffQueryHandler> _logger;
        
        public GetAllStaffQueryHandler(IStaffService staffService, ILogger<GetAllStaffQueryHandler> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<StaffMemberDto>>> Handle(GetAllStaffQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var staffMembers = await _staffService.GetAllStaffMembersAsync(request.SearchRequest, cancellationToken);

                return new ApiResponse<List<StaffMemberDto>>
                {
                    Success = true,
                    Data = staffMembers,
                    Message = $"Retrieved {staffMembers.Count} staff members"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve staff members");
                return new ApiResponse<List<StaffMemberDto>>
                {
                    Success = false,
                    Data = new List<StaffMemberDto>(),
                    Message = "Failed to retrieve staff members",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
