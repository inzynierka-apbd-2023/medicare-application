using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data;

/// <summary>
/// Shared deterministic IDs for cross-service mock data references
/// </summary>
public static class MockIds
{
    // Patient User IDs (from UserService)
    public static readonly Guid PatientUser1 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000001");
    public static readonly Guid PatientUser2 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000002");
    public static readonly Guid PatientUser3 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000003");
    public static readonly Guid PatientUser4 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000004");
    public static readonly Guid PatientUser5 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000005");
    public static readonly Guid PatientUser6 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000006");
    public static readonly Guid PatientUser7 = Guid.Parse("aaaaaaaa-0001-0001-0001-000000000007");

    // Doctor User IDs (from UserService)
    public static readonly Guid DoctorUser1 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000001");
    public static readonly Guid DoctorUser2 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000002");
    public static readonly Guid DoctorUser3 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000003");
    public static readonly Guid DoctorUser4 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000004");
    public static readonly Guid DoctorUser5 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000005");
    public static readonly Guid DoctorUser6 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000006");
    public static readonly Guid DoctorUser7 = Guid.Parse("bbbbbbbb-0002-0002-0002-000000000007");

    // Notification IDs
    public static readonly Guid Notification1 = Guid.Parse("eeee1111-1111-1111-1111-000000000001");
    public static readonly Guid Notification2 = Guid.Parse("eeee1111-1111-1111-1111-000000000002");
    public static readonly Guid Notification3 = Guid.Parse("eeee1111-1111-1111-1111-000000000003");
    public static readonly Guid Notification4 = Guid.Parse("eeee1111-1111-1111-1111-000000000004");
    public static readonly Guid Notification5 = Guid.Parse("eeee1111-1111-1111-1111-000000000005");
    public static readonly Guid Notification6 = Guid.Parse("eeee1111-1111-1111-1111-000000000006");
    public static readonly Guid Notification7 = Guid.Parse("eeee1111-1111-1111-1111-000000000007");

    public static readonly Guid[] AllPatientUserIds = { PatientUser1, PatientUser2, PatientUser3, PatientUser4, PatientUser5, PatientUser6, PatientUser7 };
    public static readonly Guid[] AllDoctorUserIds = { DoctorUser1, DoctorUser2, DoctorUser3, DoctorUser4, DoctorUser5, DoctorUser6, DoctorUser7 };
    public static readonly Guid[] AllNotificationIds = { Notification1, Notification2, Notification3, Notification4, Notification5, Notification6, Notification7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(NotificationsDbContext db)
    {
        int created = 0;

        // Notification types: 1=Appointment, 2=Message, 3=LabResults, 4=Prescription, 5=System, 6=Billing, 7=Reminder
        var notificationData = new[]
        {
            (MockIds.Notification1, MockIds.PatientUser1, "Your appointment with Dr. Carter is tomorrow at 10:00 AM", (byte)1, "AppointmentService", false, "/appointments/1", "High", DateTime.UtcNow.AddDays(2)),
            (MockIds.Notification2, MockIds.PatientUser2, "You have a new message from Dr. Chen", (byte)2, "MessagingService", false, "/messages", "Normal", DateTime.UtcNow.AddDays(7)),
            (MockIds.Notification3, MockIds.PatientUser3, "Your lab results are now available for review", (byte)3, "LabService", true, "/documents/lab-results", "High", DateTime.UtcNow.AddDays(30)),
            (MockIds.Notification4, MockIds.PatientUser4, "Your prescription has been renewed and sent to pharmacy", (byte)4, "DocumentsService", true, "/prescriptions", "Normal", DateTime.UtcNow.AddDays(14)),
            (MockIds.Notification5, MockIds.DoctorUser1, "System maintenance scheduled for tonight at 11 PM", (byte)5, "System", false, null, "Low", DateTime.UtcNow.AddDays(1)),
            (MockIds.Notification6, MockIds.PatientUser5, "Your payment of $150.00 has been processed successfully", (byte)6, "BillingService", true, "/billing/history", "Normal", DateTime.UtcNow.AddDays(30)),
            (MockIds.Notification7, MockIds.PatientUser6, "Reminder: Annual checkup due in 2 weeks", (byte)7, "AppointmentService", false, "/appointments/book", "Normal", DateTime.UtcNow.AddDays(14))
        };

        var existingNotificationIds = await db.Notifications.Select(n => n.Id).ToHashSetAsync();
        foreach (var (id, recipientId, description, type, source, isRead, actionUrl, priority, expiresAt) in notificationData)
        {
            if (!existingNotificationIds.Contains(id))
            {
                db.Notifications.Add(new Notification
                {
                    Id = id,
                    Recipient_User_Id = recipientId,
                    Description = description,
                    Type = type,
                    Source_Service = source,
                    Is_Read = isRead,
                    Action_Url = actionUrl,
                    Priority_Level = priority,
                    Creation_Date = DateTime.UtcNow.AddDays(-3 + Array.IndexOf(MockIds.AllNotificationIds, id)),
                    Expires_At = expiresAt
                });
                created++;
            }
        }

        // Add more notifications to reach 7+ per user (additional notifications for patients and doctors)
        var additionalNotifications = new[]
        {
            (Guid.NewGuid(), MockIds.PatientUser1, "Your insurance claim has been submitted", (byte)6, "BillingService", false, "/billing/claims", "Normal"),
            (Guid.NewGuid(), MockIds.PatientUser1, "Dr. Carter has updated your treatment plan", (byte)5, "MedicalRecordsService", false, "/records", "High"),
            (Guid.NewGuid(), MockIds.PatientUser2, "Your appointment has been confirmed", (byte)1, "AppointmentService", true, "/appointments", "Normal"),
            (Guid.NewGuid(), MockIds.PatientUser2, "Flu vaccination reminder for this season", (byte)7, "AppointmentService", false, "/appointments/book", "Normal"),
            (Guid.NewGuid(), MockIds.DoctorUser2, "New patient registration: Bob Smith", (byte)5, "PatientService", false, "/patients/new", "Normal"),
            (Guid.NewGuid(), MockIds.DoctorUser3, "Lab results require your review", (byte)3, "LabService", false, "/lab/review", "High"),
            (Guid.NewGuid(), MockIds.PatientUser7, "Your prescription refill is ready for pickup", (byte)4, "DocumentsService", false, "/prescriptions", "Normal"),
            // Doctor-specific notifications for DoctorUser1 (Dr. John Carter)
            (Guid.NewGuid(), MockIds.DoctorUser1, "Patient Alice Johnson has arrived for 10:00 AM appointment", (byte)1, "AppointmentService", false, "/todays-appointments", "High"),
            (Guid.NewGuid(), MockIds.DoctorUser1, "Lab results for Bob Smith are now available", (byte)3, "LabService", false, "/lab-results", "Normal"),
            (Guid.NewGuid(), MockIds.DoctorUser1, "Patient Carol Davis sent a new message", (byte)2, "MessagingService", false, "/messages", "Normal"),
            (Guid.NewGuid(), MockIds.DoctorUser1, "Prescription renewal request from Alice Johnson", (byte)4, "DocumentsService", false, "/prescriptions", "Normal"),
            // Doctor-specific notifications for DoctorUser2 (Dr. Sarah Chen)
            (Guid.NewGuid(), MockIds.DoctorUser2, "Patient Bob Smith has arrived for 2:00 PM appointment", (byte)1, "AppointmentService", false, "/todays-appointments", "High"),
            (Guid.NewGuid(), MockIds.DoctorUser2, "New appointment request from patient David Wilson", (byte)1, "AppointmentService", false, "/doctor-scheduler", "Normal"),
            (Guid.NewGuid(), MockIds.DoctorUser2, "Lab results for patient Emily Brown require review", (byte)3, "LabService", false, "/lab-results", "High")
        };

        foreach (var (id, recipientId, description, type, source, isRead, actionUrl, priority) in additionalNotifications)
        {
            var exists = await db.Notifications.AnyAsync(n => n.Recipient_User_Id == recipientId && n.Description == description);
            if (!exists)
            {
                db.Notifications.Add(new Notification
                {
                    Id = id,
                    Recipient_User_Id = recipientId,
                    Description = description,
                    Type = type,
                    Source_Service = source,
                    Is_Read = isRead,
                    Action_Url = actionUrl,
                    Priority_Level = priority,
                    Creation_Date = DateTime.UtcNow.AddHours(-additionalNotifications.ToList().IndexOf((id, recipientId, description, type, source, isRead, actionUrl, priority)) - 1),
                    Expires_At = DateTime.UtcNow.AddDays(7)
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} notification records.");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All notification mock data already exists.");
        }
    }
}
