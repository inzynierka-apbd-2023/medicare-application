using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using AppointmentService.Data;
using AppointmentService.Models;
using MassTransit;
using Medicare.Messaging.Contracts;
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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMediator _mediator;
    private readonly IRequestClient<IGetPatient> _patientRequestClient;

    public AppointmentsController(AppointmentDbContext db, IPublishEndpoint publishEndpoint, IMediator mediator, IRequestClient<IGetPatient> patientRequestClient)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _mediator = mediator;
        _patientRequestClient = patientRequestClient;
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
       
        await _publishEndpoint.Publish<IAppointmentCreated>(new
        {
            AppointmentId = appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.ScheduledAt,
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

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


        var response = await _patientRequestClient.GetResponse<IPatientProfile>(new { PatientId = patientId });
        var profile = response.Message;
        
        var patientDto = new PatientProfileDto
        {
            Id = profile.PatientId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email,
            Phone = profile.Phone,
            DateOfBirth = profile.DateOfBirth ?? default,
            Gender = profile.Gender,
            AddressLine1 = profile.AddressLine1,
            AddressLine2 = profile.AddressLine2,
            City = profile.City,
            State = profile.State,
            ZipCode = profile.ZipCode,
            Country = profile.Country
        };
        
        foreach (var a in appointments)
        {
            a.Patient = patientDto;
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

        var uniquePatientIds = appointments.Select(a => a.PatientId).Distinct();
        
        var profileTasks = uniquePatientIds.Select(async pid => 
        {
            var response = await _patientRequestClient.GetResponse<IPatientProfile>(new { PatientId = pid });
            return response.Message;
        });

        var profiles = await Task.WhenAll(profileTasks);
        var profileDict = profiles.Where(p => p != null).ToDictionary(p => p!.PatientId, p => new PatientProfileDto
        {
            Id = p!.PatientId,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Email = p.Email,
            Phone = p.Phone,
            DateOfBirth = p.DateOfBirth ?? default,
            Gender = p.Gender,
            AddressLine1 = p.AddressLine1,
            AddressLine2 = p.AddressLine2,
            City = p.City,
            State = p.State,
            ZipCode = p.ZipCode,
            Country = p.Country
        });

        foreach (var a in appointments)
        {
            if (profileDict.TryGetValue(a.PatientId, out var patientDto))
            {
                a.Patient = patientDto;
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
        
        await _publishEndpoint.Publish<IAppointmentUpdated>(new 
        { 
            AppointmentId = appointment.Id,
            appointment.DoctorId,
            appointment.Status,
            appointment.UpdatedAt,
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return Ok(appointment);
    }
    
    [HttpPost("{id}/mock-payment")]
    public async Task<IActionResult> ProcessMockPayment(Guid id, [FromBody] MockPaymentRequest req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound("Appointment not found");

        if (appointment.IsPaid) return Ok(new { Success = true, Message = "Already paid" });

        await _publishEndpoint.Publish<IBillingPaymentInitiated>(new
        {
            AppointmentId = id,
            req.PatientId,
            req.PaymentMethod,
            Timestamp = DateTime.UtcNow
        });
        
        await _db.SaveChangesAsync();

        return Accepted(new { Success = true, Message = "Payment initiated" });
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentRequestDto req)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        if (req.Description != null) appointment.Notes = req.Description; 
        
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

        await _publishEndpoint.Publish<IAppointmentRated>(new
        {
            AppointmentId = appointment.Id,
            appointment.DoctorId,
            appointment.PatientId,
            req.Rating,
            req.Description,
            OccurredAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return Ok(new { Message = "Rating submitted" });
    }
}

public record CreateAppointmentRequest(Guid PatientId, Guid DoctorId, DateTime ScheduledAt, DateTime ScheduledEndAt, string? AppointmentType, string? Notes, Guid? ServiceId, string? Category, string? Room);
public record UpdateStatusRequest(string Status);
public record MockPaymentRequest(Guid PatientId, string PaymentMethod);
public record UpdateAppointmentRequestDto(string? Description, DateTime? ScheduledAt, DateTime? ScheduledEndAt, string? AppointmentType, Guid? ServiceId, string? Category, string? Room);
public record RateAppointmentRequest(byte Rating, string? Description);
