using AppointmentService.Features.DoctorSchedule.DTOs;

namespace AppointmentService.Features.DoctorSchedule.Services;

public interface IPatientService
{
    Task<PatientDto?> GetPatientAsync(string patientId, CancellationToken cancellationToken = default);
}

public class PatientService : IPatientService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public PatientService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        var baseUrl = _configuration["Services:PatientService:BaseUrl"] ?? "http://localhost:8081";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<PatientDto?> GetPatientAsync(string patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/patient/patients/{patientId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var patientData = await response.Content.ReadFromJsonAsync<PatientDto>(cancellationToken);
            return patientData;
        }
        catch
        {
            // Log error in production
            return null;
        }
    }
}
