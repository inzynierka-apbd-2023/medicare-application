using System;

namespace PatientService.Features.Metrics.DTOs;

/// <summary>
/// Response model for patient metrics used by Owner dashboard.
/// Logic/population will be implemented later.
/// </summary>
public class PatientMetricsResponse
{
    /// <summary>Total number of active patients in the selected period / overall.</summary>
    public int TotalActivePatients { get; set; }
    /// <summary>Number of newly registered patients within the date range.</summary>
    public int NewPatients { get; set; }
    /// <summary>Patient retention rate percentage (0-100).</summary>
    public decimal RetentionRate { get; set; }
    /// <summary>Average rating given by patients to doctors (0-5).</summary>
    public decimal AverageRating { get; set; }
    /// <summary>Total number of ratings submitted within / up to the period.</summary>
    public int TotalRatings { get; set; }
    /// <summary>Metrics calculation start date (normalized).</summary>
    public DateTime? StartDate { get; set; }
    /// <summary>Metrics calculation end date (normalized).</summary>
    public DateTime? EndDate { get; set; }
    /// <summary>Indicates metrics are placeholder values (since logic not implemented yet).</summary>
    public bool IsStub { get; set; } = true;
}
