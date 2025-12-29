using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Features.DoctorSchedule.Services;

public interface IMedicalRecordsService
{
    Task<MedicalRecordDto?> GetMedicalRecordAsync(Guid patientId, CancellationToken cancellationToken = default);
}

public class MedicalRecordsService : IMedicalRecordsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public MedicalRecordsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        var baseUrl = _configuration["Services:MedicalRecordsService:BaseUrl"] ?? "http://localhost:8083";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<MedicalRecordDto?> GetMedicalRecordAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/medical-records/patients/{patientId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var medicalRecord = await response.Content.ReadFromJsonAsync<MedicalRecordDto>(cancellationToken);
            return medicalRecord;
        }
        catch
        {
            // Log error in production
            return null;
        }
    }
}
