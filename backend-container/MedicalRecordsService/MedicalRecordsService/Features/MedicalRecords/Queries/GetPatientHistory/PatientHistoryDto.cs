using MedicalRecordsService.Models;

namespace MedicalRecordsService.Features.MedicalRecords.Queries.GetPatientHistory;

public class PatientHistoryDto
{
    public Guid PatientId { get; set; }
    public List<MedicalRecord> Records { get; set; } = new();
    public List<Diagnosis> Conditions { get; set; } = new();
    public List<Prescription> Medications { get; set; } = new();
    public List<VitalSigns> Vitals { get; set; } = new();
}
