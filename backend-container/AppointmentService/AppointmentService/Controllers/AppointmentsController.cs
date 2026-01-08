using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentService.Data;
using AppointmentService.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using AppointmentService.Features.Scheduler.DTOs;
using AppointmentService.Features.Scheduler.Queries;
using MediatR;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/[controller]")]
public class AppointmentsController : ControllerBase
{

    private readonly AppointmentDbContext _db;
    private readonly IConnection _mqConnection;
    private readonly ILogger<AppointmentsController> _logger;
    private readonly AppointmentService.Services.IBillingServiceClient _billingClient;
    private readonly IMediator _mediator;

    public AppointmentsController(AppointmentDbContext db, IConnection mqConnection, ILogger<AppointmentsController> logger, AppointmentService.Services.IBillingServiceClient billingClient, IMediator mediator)
    {
        _db = db;
        _mqConnection = mqConnection;
        _logger = logger;
        _billingClient = billingClient;
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest req)
    {
        if (req.PatientId == Guid.Empty || req.DoctorId == Guid.Empty)
            return BadRequest("PatientId and DoctorId are required");
        if (req.ScheduledAt == default || req.ScheduledEndAt == default || req.ScheduledEndAt <= req.ScheduledAt)
            return BadRequest("Invalid scheduled times");

        try
        {
            // 0. Pre-generate ID
            var apptId = Guid.NewGuid();

            // 1. Sync Call to Billing Service
            var billingResult = await _billingClient.EvaluateAppointmentAsync(apptId, req.PatientId, req.ScheduledAt);
            _logger.LogInformation("Billing Check for {Id}: IsPaid={IsPaid}, Amount={Amount}", apptId, billingResult.IsPaid, billingResult.AmountCents);

            var appointment = new Appointment
            {
                Id = apptId,
                PatientId = req.PatientId,
                DoctorId = req.DoctorId,
                ScheduledAt = req.ScheduledAt,
                ScheduledEndAt = req.ScheduledEndAt,
                AppointmentType = req.AppointmentType,
                Notes = req.Notes,
                ServiceId = req.ServiceId,
                Category = req.Category,
                Room = req.Room,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPaid = billingResult.IsPaid,
                PaymentProcessed = true // Flag processed immediately
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            // Publish event (still useful for other services, but not for billing loop)
            await PublishAppointmentCreatedAsync(appointment);

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { message = "Database update failed", error = ex.Message, inner = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to create appointment", error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<SchedulerStatsResponse>> GetStats([FromQuery] Guid? doctorId, [FromQuery] Guid? patientId)
    {
        var query = new GetSchedulerStatsQuery { DoctorId = doctorId, PatientId = patientId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();
        var now = DateTime.Now;
        if ((appointment.Status == "Scheduled" || appointment.Status == "Confirmed") && appointment.ScheduledEndAt < now)
        {
            // Effective overdue on read
            appointment.Status = "Overdue";
        }
        return Ok(appointment);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        try
        {
            var now = DateTime.Now;
            var appointments = await _db.Appointments
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.ScheduledAt)
                .ToListAsync();
            foreach (var a in appointments)
            {
                if ((a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
                {
                    a.Status = "Overdue";
                }
            }
            return Ok(appointments);
        }
        catch
        {
            // Graceful fallback if database not ready
            return Ok(Array.Empty<Appointment>());
        }
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctorId(Guid doctorId)
    {
        try
        {
            var now = DateTime.Now;
            var appointments = await _db.Appointments
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.ScheduledAt)
                .ToListAsync();
            foreach (var a in appointments)
            {
                if ((a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
                {
                    a.Status = "Overdue";
                }
            }
            return Ok(appointments);
        }
        catch
        {
            return Ok(Array.Empty<Appointment>());
        }
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = req.Status;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(appointment);
    }
    
    [HttpPost("{id}/mock-payment")]
    [Authorize]
    public async Task<IActionResult> ProcessMockPayment(Guid id, [FromBody] MockPaymentRequest req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound("Appointment not found");

        if (appointment.IsPaid) return BadRequest("Appointment is already paid");

        // 1. Call Billing Service to record sync payment
        var success = await _billingClient.RecordMockPaymentAsync(id, req.PatientId, req.PaymentMethod);
        
        if (!success)
        {
            return BadRequest("Billing service failed to record payment");
        }

        // 2. Update local state immediately
        appointment.IsPaid = true;
        // Optionally update payment processed if not set
        appointment.PaymentProcessed = true; 
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Successfully processed mock payment for {Id}. Local DB updated.", id);

        return Ok(new { Success = true });
    }

    [HttpGet("analytics/today")]
    public async Task<IActionResult> GetTodaysAnalytics()
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todaysAppointments = await _db.Appointments
                .Where(a => a.ScheduledAt >= today && a.ScheduledAt < tomorrow)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new { Date = today, Statistics = todaysAppointments });
        }
        catch
        {
            return Ok(new { Date = DateTime.Today, Statistics = Array.Empty<object>() });
        }
    }

    private async Task PublishAppointmentCreatedAsync(Appointment appointment)
    {
        try
        {
            await using var channel = await _mqConnection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync("appointment.events", ExchangeType.Topic, durable: true);

            var evt = new
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                ScheduledAt = appointment.ScheduledAt,
                OccurredAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties();
            await channel.BasicPublishAsync(exchange: "appointment.events",
                                 routingKey: "appointment.created",
                                 mandatory: false,
                                 basicProperties: props,
                                 body: body);
            
            _logger.LogInformation("Published appointment.created for {Id}", appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish appointment.created for {Id}", appointment.Id);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentRequestDto req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        // Update fields that are allowed to be modified
        if (req.Description != null) appointment.Notes = req.Description; // Allow clearing if empty string sent? or only if not null
        
        if (req.ScheduledAt.HasValue) appointment.ScheduledAt = req.ScheduledAt.Value;
        if (req.ScheduledEndAt.HasValue) appointment.ScheduledEndAt = req.ScheduledEndAt.Value;
        if (!string.IsNullOrEmpty(req.AppointmentType)) appointment.AppointmentType = req.AppointmentType;
        if (req.ServiceId.HasValue) appointment.ServiceId = req.ServiceId.Value;
        if (!string.IsNullOrEmpty(req.Category)) appointment.Category = req.Category;
        if (req.Room != null) appointment.Room = req.Room; 
        
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();

        return Ok(appointment);
    }
}

public record CreateAppointmentRequest(Guid PatientId, Guid DoctorId, DateTime ScheduledAt, DateTime ScheduledEndAt, string? AppointmentType, string? Notes, Guid? ServiceId, string? Category, string? Room);
public record UpdateStatusRequest(string Status);
public record MockPaymentRequest(Guid PatientId, string PaymentMethod);
public record UpdateAppointmentRequestDto(string? Description, DateTime? ScheduledAt, DateTime? ScheduledEndAt, string? AppointmentType, Guid? ServiceId, string? Category, string? Room);
