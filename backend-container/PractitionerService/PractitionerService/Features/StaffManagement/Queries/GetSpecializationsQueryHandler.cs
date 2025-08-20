using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Data;
using Microsoft.EntityFrameworkCore;

namespace PractitionerService.Features.StaffManagement.Queries
{
    public class GetSpecializationsQueryHandler : IRequestHandler<GetSpecializationsQuery, ApiResponse<List<SpecializationDto>>>
    {
        private readonly PractitionerDbContext _context;
        private readonly ILogger<GetSpecializationsQueryHandler> _logger;
        
        public GetSpecializationsQueryHandler(PractitionerDbContext context, ILogger<GetSpecializationsQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<SpecializationDto>>> Handle(GetSpecializationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var specializations = await _context.Specializations
                    .Select(s => new SpecializationDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = null, // Add if description field exists
                        IsPrimary = false // This would need business logic
                    })
                    .ToListAsync(cancellationToken);

                return new ApiResponse<List<SpecializationDto>>
                {
                    Success = true,
                    Data = specializations,
                    Message = $"Retrieved {specializations.Count} specializations"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve specializations");
                return new ApiResponse<List<SpecializationDto>>
                {
                    Success = false,
                    Data = new List<SpecializationDto>(),
                    Message = "Failed to retrieve specializations",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
