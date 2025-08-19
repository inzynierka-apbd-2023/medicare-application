using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Features.Metrics.DTOs;

namespace PatientService.Features.Metrics.Services;

public class PatientMetricsService : IPatientMetricsService
{
    private readonly PatientDbContext _db;
    public PatientMetricsService(PatientDbContext db) => _db = db;

    public async Task<PatientMetricsResponse> GetMetricsAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
    {
    var activePatientIds = await _db.PatientStatuses.AsNoTracking()
            .GroupBy(s => s.PatientId)
            .Select(g => g.OrderByDescending(s => s.EffectiveAt).First())
            .Where(latest => latest.Status == "Active")
            .Select(latest => latest.PatientId)
            .ToListAsync(ct);

        var totalActive = activePatientIds.Count;

    var newPatients = await _db.Patients.AsNoTracking()
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .CountAsync(ct);

    var retained = await _db.Patients.AsNoTracking()
            .Where(p => p.CreatedAt < startDate && activePatientIds.Contains(p.Id))
            .CountAsync(ct);
    var prior = await _db.Patients.AsNoTracking().Where(p => p.CreatedAt < startDate).CountAsync(ct);
        decimal retention = prior == 0 ? 0 : (decimal)retained / prior * 100m;

        return new PatientMetricsResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalActivePatients = totalActive,
            NewPatients = newPatients,
            RetentionRate = decimal.Round(retention, 2),
            AverageRating = 0,
            TotalRatings = 0,
            IsStub = false
        };
    }
}
