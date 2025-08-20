using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Services;

namespace PractitionerService.Features.StaffManagement.Commands
{
    public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand, ApiResponse<bool>>
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<DeleteStaffCommandHandler> _logger;
        
        public DeleteStaffCommandHandler(IStaffService staffService, ILogger<DeleteStaffCommandHandler> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _staffService.DeleteStaffMemberAsync(request.Id, cancellationToken);

                if (!result)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Staff member not found",
                        Errors = new List<string> { $"No staff member found with ID: {request.Id}" }
                    };
                }

                _logger.LogInformation("Soft deleted staff member with ID {StaffId}", request.Id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Staff member deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete staff member with ID {StaffId}", request.Id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Failed to delete staff member",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
