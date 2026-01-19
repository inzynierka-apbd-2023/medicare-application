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
using AppointmentService.Services;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/appointment/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{

    private readonly AppointmentDbContext _db;
    private readonly IConnection _mqConnection;
    private readonly AppointmentService.Services.IBillingServiceClient _billingClient;
    private readonly IMediator _mediator;
    private readonly IPatientProfileClient _patientProfileClient;

    public AppointmentsController(AppointmentDbContext db, IConnection mqConnection, AppointmentService.Services.IBillingServiceClient billingClient, IMediator mediator, IPatientProfileClient patientProfileClient)
    {
        _db = db;
        _mqConnection = mqConnection;
        _billingClient = billingClient;
        _mediator = mediator;
        _patientProfileClient = patientProfileClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest req)
    {
        if (req.PatientId == Guid.Empty || req.DoctorId == Guid.Empty)
            return BadRequest("PatientId and DoctorId are required");
        if (req.ScheduledAt == default || req.ScheduledEndAt == default || req.ScheduledEndAt <= req.ScheduledAt)
            return BadRequest("Invalid scheduled times");

        // 0. Pre-generate ID
        var apptId = Guid.NewGuid();

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
            IsPaid = false,
            PaymentProcessed = false
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        await PublishAppointmentCreatedAsync(appointment);

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
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
        var now = DateTime.Now;
        var appointments = await _db.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();

        var patientProfiles = await _patientProfileClient.GetPatientProfilesAsync(new[] { patientId });
        var patientProfile = patientProfiles.FirstOrDefault();

        foreach (var a in appointments)
        {
            a.Patient = patientProfile;
            if ((a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
            {
                a.Status = "Overdue";
            }
        }

        return Ok(appointments);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctorId(Guid doctorId)
    {
        var now = DateTime.Now;
        var appointments = await _db.Appointments
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();

        var patientIds = appointments.Select(a => a.PatientId).Distinct();
        var patients = await _patientProfileClient.GetPatientProfilesAsync(patientIds);
        var patientMap = patients.ToDictionary(p => p.PatientId);

        foreach (var a in appointments)
        {
            if (patientMap.TryGetValue(a.PatientId, out var p))
            {
                a.Patient = p;
            }
            if ((a.Status == "Scheduled" || a.Status == "Confirmed") && a.ScheduledEndAt < now)
            {
                a.Status = "Overdue";
            }
        }

        return Ok(appointments);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = req.Status;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await PublishAppointmentUpdatedAsync(appointment);

        return Ok(appointment);
    }
    
    [HttpPost("{id}/mock-payment")]
    public async Task<IActionResult> ProcessMockPayment(Guid id, [FromBody] MockPaymentRequest req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound("Appointment not found");

        if (appointment.IsPaid) return Ok(new { Success = true, Message = "Already paid" });

        // 1. Initiate payment via Billing Service (Async)
        var (success, error) = await _billingClient.RecordMockPaymentAsync(id, req.PatientId, req.PaymentMethod);
        
        if (!success)
        {
            return BadRequest(new { Message = "Billing service failed to record payment", Details = error });
        }

        // 2. Update local state immediately
        appointment.IsPaid = true;
        // Optionally update payment processed if not set
        appointment.PaymentProcessed = true; 
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();


        return Ok(new { Success = true });
    }

    [HttpGet("analytics/today")]
    public async Task<IActionResult> GetTodaysAnalytics()
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

    private async Task PublishAppointmentCreatedAsync(Appointment appointment)
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
    }

    private async Task PublishAppointmentUpdatedAsync(Appointment appointment)
    {
        await using var channel = await _mqConnection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("appointment.events", ExchangeType.Topic, durable: true);

        var evt = new
        {
            AppointmentId = appointment.Id,
            DoctorId = appointment.DoctorId,
            Status = appointment.Status,
            UpdatedAt = appointment.UpdatedAt,
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties();
        await channel.BasicPublishAsync(exchange: "appointment.events",
                                routingKey: "appointment.updated",
                                mandatory: false,
                                basicProperties: props,
                                body: body);

    }

    [HttpPut("{id}")]
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

    [HttpPost("{id}/rate")]
    public async Task<IActionResult> RateAppointment(Guid id, [FromBody] RateAppointmentRequest req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        // 1. Create Rate entity
        var rate = new Rate
        {
            Id = Guid.NewGuid(),
            Rate_Value = req.Rating,
            Description = req.Description,
            Patient_User_Id = appointment.PatientId,
            Doctor_User_Id = appointment.DoctorId,
            Appointment_Id = appointment.Id,
            Rated_At = DateTime.UtcNow,
            Is_Anonymous = false
        };

        _db.Set<Rate>().Add(rate);
        await _db.SaveChangesAsync();

        // 2. Publish event
        await PublishAppointmentRatedAsync(appointment, req.Rating);

        return Ok(rate);
    }

    private async Task PublishAppointmentRatedAsync(Appointment appointment, int rating)
    {
        await using var channel = await _mqConnection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("appointment.events", ExchangeType.Topic, durable: true);

        var evt = new
        {
            AppointmentId = appointment.Id,
            DoctorId = appointment.DoctorId,
            Rating = rating,
            OccurredAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties();
        await channel.BasicPublishAsync(exchange: "appointment.events",
                                routingKey: "appointment.rated",
                                mandatory: false,
                                basicProperties: props,
                                body: body);
    }
}

public record CreateAppointmentRequest(Guid PatientId, Guid DoctorId, DateTime ScheduledAt, DateTime ScheduledEndAt, string? AppointmentType, string? Notes, Guid? ServiceId, string? Category, string? Room);
public record UpdateStatusRequest(string Status);
public record MockPaymentRequest(Guid PatientId, string PaymentMethod);
public record UpdateAppointmentRequestDto(string? Description, DateTime? ScheduledAt, DateTime? ScheduledEndAt, string? AppointmentType, Guid? ServiceId, string? Category, string? Room);
public record RateAppointmentRequest(byte Rating, string? Description);
