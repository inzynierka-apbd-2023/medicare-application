using BillingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers;

[ApiController]
[Route("api/billing/internal")]
[Authorize]
public class InternalBillingController : ControllerBase
{
    private readonly AppointmentBillingService _billingService;

    public InternalBillingController(AppointmentBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost("evaluate-appointment")]
    public async Task<IActionResult> EvaluateAppointment([FromBody] EvaluateAppointmentRequest req)
    {
        var result = await _billingService.EvaluateAndRecordPaymentAsync(req.AppointmentId, req.PatientId, req.ScheduledAt);
        return Ok(new 
        { 
            IsPaid = result.IsFree || result.AmountCents == 0,
            AmountCents = result.AmountCents,
            PlanCode = result.PlanCode
        });
    }
}

public record EvaluateAppointmentRequest(Guid AppointmentId, Guid PatientId, DateTime ScheduledAt);
