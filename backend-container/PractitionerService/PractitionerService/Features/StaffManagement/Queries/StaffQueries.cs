using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;

namespace PractitionerService.Features.StaffManagement.Queries;

public class GetAllStaffQuery : IRequest<ApiResponse<List<StaffMemberDto>>>
{
    public StaffSearchRequest SearchRequest { get; set; } = new();
}

public class GetStaffByIdQuery : IRequest<ApiResponse<StaffMemberDto>>
{
    public Guid Id { get; set; }
}

public class GetStaffByRoleQuery : IRequest<ApiResponse<List<StaffMemberDto>>>
{
    public string Role { get; set; } = default!;
}

public class GetSpecializationsQuery : IRequest<ApiResponse<List<SpecializationDto>>>
{
}

public class GetServicesQuery : IRequest<ApiResponse<List<ServiceDto>>>
{
}
