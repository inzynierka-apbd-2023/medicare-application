using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;

namespace PractitionerService.Features.StaffManagement.Commands;

public class CreateStaffCommand : IRequest<ApiResponse<StaffMemberDto>>
{
    public CreateStaffRequest Request { get; set; } = default!;
}

public class UpdateStaffCommand : IRequest<ApiResponse<StaffMemberDto>>
{
    public UpdateStaffRequest Request { get; set; } = default!;
}

public class DeleteStaffCommand : IRequest<ApiResponse<bool>>
{
    public string Id { get; set; } = default!;
}
