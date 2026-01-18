using MediatR;
using PractitionerService.Features.StaffManagement.DTOs;
using PractitionerService.Data;
using Microsoft.EntityFrameworkCore;

namespace PractitionerService.Features.StaffManagement.Queries
{
    public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, ApiResponse<List<ServiceDto>>>
    {
        private readonly PractitionerDbContext _context;
        private readonly ILogger<GetServicesQueryHandler> _logger;
        
        public GetServicesQueryHandler(PractitionerDbContext context, ILogger<GetServicesQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<ServiceDto>>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var services = await _context.Services
                    .Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        DurationMinutes = 30, // Default duration
                        IsActive = true
                    })
                    .ToListAsync(cancellationToken);

                return new ApiResponse<List<ServiceDto>>
                {
                    Success = true,
                    Data = services,
                    Message = $"Retrieved {services.Count} services"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve services");
                return new ApiResponse<List<ServiceDto>>
                {
                    Success = false,
                    Data = new List<ServiceDto>(),
                    Message = "Failed to retrieve services",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
