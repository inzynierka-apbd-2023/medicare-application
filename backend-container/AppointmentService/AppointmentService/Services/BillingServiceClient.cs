namespace AppointmentService.Services;

public interface IBillingServiceClient
{
    Task<(bool IsPaid, long AmountCents, string PlanCode)> EvaluateAppointmentAsync(Guid appointmentId, Guid patientId, DateTime scheduledAt);
    Task<bool> RecordMockPaymentAsync(Guid appointmentId, Guid patientId, string method);
}

public class BillingServiceClient : IBillingServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<BillingServiceClient> _logger;

    public BillingServiceClient(HttpClient http, ILogger<BillingServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<(bool IsPaid, long AmountCents, string PlanCode)> EvaluateAppointmentAsync(Guid appointmentId, Guid patientId, DateTime scheduledAt)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/billing/internal/evaluate-appointment", new { AppointmentId = appointmentId, PatientId = patientId, ScheduledAt = scheduledAt });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BillingEvaluationResult>();
                if (result != null)
                {
                    return (result.IsPaid, result.AmountCents, result.PlanCode);
                }
            }
            _logger.LogError("Failed to call BillingService. Status: {Status}", response.StatusCode);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Failed to call BillingService");
        }
        
        // Fallback: Not paid, default price
        return (false, 30000, "FREE");
    }

    public async Task<bool> RecordMockPaymentAsync(Guid appointmentId, Guid patientId, string method)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/billing/payment/mock", new { AppointmentId = appointmentId, PatientId = patientId, PaymentMethod = method });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call BillingService Mock Payment");
            return false;
        }
    }

    private record BillingEvaluationResult(bool IsPaid, long AmountCents, string PlanCode);
}
