using MediatR;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;

namespace PatientService.Features.Patients.Queries.GetPatientsBatch;

public class BatchPatientsHandler : IRequestHandler<GetPatientsBatchQuery, List<PatientOverview>>
{
    private readonly PatientDbContext _db;
    private readonly IRequestClient<IGetUsers> _client;

    public BatchPatientsHandler(PatientDbContext db, IRequestClient<IGetUsers> client)
    {
        _db = db;
        _client = client;
    }

    public async Task<List<PatientOverview>> Handle(GetPatientsBatchQuery request, CancellationToken cancellationToken)
    {
        if (request.PatientIds == null || !request.PatientIds.Any())
        {
            return new List<PatientOverview>();
        }

        var distinctIds = request.PatientIds.Distinct().ToList();

        var patients = await _db.Patients
            .AsNoTracking()
            .Where(p => distinctIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (!patients.Any()) return new List<PatientOverview>();

        var userIds = patients.Select(p => p.UserId).Distinct().ToList();
        var userMap = new Dictionary<Guid, IUserResponse>();

        var response = await _client.GetResponse<IUsersResponse>(new { UserIds = userIds }, cancellationToken);
        if (response.Message.Users != null)
        {
            foreach (var u in response.Message.Users)
            {
                userMap[u.Id] = u;
            }
        }

        var result = new List<PatientOverview>();
        foreach (var p in patients)
        {
            userMap.TryGetValue(p.UserId, out var user);
            
            result.Add(new PatientOverview
            {
                PatientId = p.Id,
                UserId = p.UserId,
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                Email = user?.Email,
                Phone = user?.Phone,
                DateOfBirth = user?.DateOfBirth,
                Gender = user?.Gender,
                AddressLine1 = user?.AddressLine1,
                AddressLine2 = user?.AddressLine2,
                City = user?.City,
                State = user?.State,
                ZipCode = user?.ZipCode,
                Country = user?.Country,
                CurrentStatus = "Active"
            });
        }

        return result;
    }

    private string? FormatAddress(IUserResponse? user)
    {
        if (user == null) return null;
        var parts = new[] { user.AddressLine1, user.City, user.Country }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        return string.Join(", ", parts);
    }
}
