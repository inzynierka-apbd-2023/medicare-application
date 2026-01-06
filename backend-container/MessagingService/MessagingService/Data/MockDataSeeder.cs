using Microsoft.EntityFrameworkCore;
using MessagingService.Models;

namespace MessagingService.Data;

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

    // Appointment IDs (for related entity references)
    public static readonly Guid Appointment1 = Guid.Parse("55555555-5555-5555-5555-000000000001");
    public static readonly Guid Appointment2 = Guid.Parse("55555555-5555-5555-5555-000000000002");
    public static readonly Guid Appointment3 = Guid.Parse("55555555-5555-5555-5555-000000000003");

    // Message IDs
    public static readonly Guid Message1 = Guid.Parse("cccc1111-1111-1111-1111-000000000001");
    public static readonly Guid Message2 = Guid.Parse("cccc1111-1111-1111-1111-000000000002");
    public static readonly Guid Message3 = Guid.Parse("cccc1111-1111-1111-1111-000000000003");
    public static readonly Guid Message4 = Guid.Parse("cccc1111-1111-1111-1111-000000000004");
    public static readonly Guid Message5 = Guid.Parse("cccc1111-1111-1111-1111-000000000005");
    public static readonly Guid Message6 = Guid.Parse("cccc1111-1111-1111-1111-000000000006");
    public static readonly Guid Message7 = Guid.Parse("cccc1111-1111-1111-1111-000000000007");

    // Thread IDs
    public static readonly Guid Thread1 = Guid.Parse("dddd1111-1111-1111-1111-000000000001");
    public static readonly Guid Thread2 = Guid.Parse("dddd1111-1111-1111-1111-000000000002");
    public static readonly Guid Thread3 = Guid.Parse("dddd1111-1111-1111-1111-000000000003");
    public static readonly Guid Thread4 = Guid.Parse("dddd1111-1111-1111-1111-000000000004");
    public static readonly Guid Thread5 = Guid.Parse("dddd1111-1111-1111-1111-000000000005");
    public static readonly Guid Thread6 = Guid.Parse("dddd1111-1111-1111-1111-000000000006");
    public static readonly Guid Thread7 = Guid.Parse("dddd1111-1111-1111-1111-000000000007");

    public static readonly Guid[] AllPatientUserIds = { PatientUser1, PatientUser2, PatientUser3, PatientUser4, PatientUser5, PatientUser6, PatientUser7 };
    public static readonly Guid[] AllDoctorUserIds = { DoctorUser1, DoctorUser2, DoctorUser3, DoctorUser4, DoctorUser5, DoctorUser6, DoctorUser7 };
    public static readonly Guid[] AllMessageIds = { Message1, Message2, Message3, Message4, Message5, Message6, Message7 };
    public static readonly Guid[] AllThreadIds = { Thread1, Thread2, Thread3, Thread4, Thread5, Thread6, Thread7 };
}

public static class MockDataSeeder
{
    public static async Task SeedAsync(MessagingDbContext db)
    {
        int created = 0;

        // Seed Direct Messages (patient <-> doctor communication)
        var messageData = new (Guid id, Guid senderId, string senderName, Guid recipientId, string recipientName, string subject, string content, string msgType, string priority, bool isRead, Guid? relatedId, string? relatedType)[]
        {
            (MockIds.Message1, MockIds.PatientUser1, "Alice Johnson", MockIds.DoctorUser1, "Dr. John Carter", "Question about medication", "Dr. Carter, I have a question about the dosage of my new medication. Should I take it before or after meals?", "Medical", "Normal", true, MockIds.Appointment1, "Appointment"),
            (MockIds.Message2, MockIds.DoctorUser1, "Dr. John Carter", MockIds.PatientUser1, "Alice Johnson", "RE: Question about medication", "Hello Alice, please take the medication with food to minimize stomach upset. Let me know if you have any other questions.", "Medical", "Normal", true, MockIds.Appointment1, "Appointment"),
            (MockIds.Message3, MockIds.PatientUser2, "Bob Smith", MockIds.DoctorUser2, "Dr. Sarah Chen", "Appointment reschedule request", "Hello Dr. Chen, I need to reschedule my appointment next week. Is Friday available?", "Appointment", "Normal", false, MockIds.Appointment2, "Appointment"),
            (MockIds.Message4, MockIds.DoctorUser3, "Dr. Michael Brown", MockIds.PatientUser3, "Charlie Davis", "Lab results ready", "Your recent lab results are now available. Everything looks normal. We can discuss at your next visit.", "Medical", "High", true, null, null),
            (MockIds.Message5, MockIds.PatientUser4, "Diana Evans", MockIds.DoctorUser4, "Dr. Emily Thompson", "Follow-up question", "Dr. Thompson, my child still has a mild cough after the visit. Should I be concerned?", "Medical", "Normal", false, null, null),
            (MockIds.Message6, MockIds.DoctorUser5, "Dr. David Wilson", MockIds.PatientUser5, "Evan Foster", "Post-procedure instructions", "Please remember to keep the knee elevated and apply ice for 20 minutes every 2 hours.", "Medical", "High", true, MockIds.Appointment3, "Appointment"),
            (MockIds.Message7, MockIds.PatientUser6, "Fiona Green", MockIds.DoctorUser6, "Dr. Lisa Anderson", "Insurance question", "Dr. Anderson, I have a question about my insurance coverage for the MRI. Can your office help?", "General", "Normal", false, null, null)
        };

        var existingMessageIds = await db.Messages.Select(m => m.Id).ToHashSetAsync();
        foreach (var (id, senderId, senderName, recipientId, recipientName, subject, content, msgType, priority, isRead, relatedId, relatedType) in messageData)
        {
            if (!existingMessageIds.Contains(id))
            {
                db.Messages.Add(new Message
                {
                    Id = id,
                    SenderId = senderId,
                    RecipientId = recipientId,
                    SenderName = senderName,
                    RecipientName = recipientName,
                    Subject = subject,
                    Content = content,
                    MessageType = msgType,
                    Priority = priority,
                    IsRead = isRead,
                    SentAt = DateTime.UtcNow.AddDays(-7 + Array.IndexOf(MockIds.AllMessageIds, id)),
                    ReadAt = isRead ? DateTime.UtcNow.AddDays(-6 + Array.IndexOf(MockIds.AllMessageIds, id)) : null,
                    RelatedEntityId = relatedId,
                    RelatedEntityType = relatedType,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                });
                created++;
            }
            else
            {
                // Update existing message if names are missing
                var existingMsg = await db.Messages.FindAsync(id);
                if (existingMsg != null && (string.IsNullOrEmpty(existingMsg.SenderName) || string.IsNullOrEmpty(existingMsg.RecipientName)))
                {
                    existingMsg.SenderName = senderName;
                    existingMsg.RecipientName = recipientName;
                    created++; // Increment to trigger SaveChanges
                }
            }
        }

        // Seed Message Threads (group conversations)
        var threadData = new[]
        {
            (MockIds.Thread1, "Care Team Discussion - Alice Johnson", MockIds.DoctorUser1),
            (MockIds.Thread2, "Post-Surgery Follow-up", MockIds.DoctorUser5),
            (MockIds.Thread3, "Lab Results Review", MockIds.DoctorUser3),
            (MockIds.Thread4, "Prescription Renewal Request", MockIds.PatientUser4),
            (MockIds.Thread5, "Appointment Scheduling", MockIds.PatientUser2),
            (MockIds.Thread6, "Medical Records Request", MockIds.PatientUser6),
            (MockIds.Thread7, "Treatment Plan Discussion", MockIds.DoctorUser7)
        };

        var existingThreadIds = await db.MessageThreads.Select(t => t.Id).ToHashSetAsync();
        foreach (var (id, subject, initiatorId) in threadData)
        {
            if (!existingThreadIds.Contains(id))
            {
                db.MessageThreads.Add(new MessageThread
                {
                    Id = id,
                    Subject = subject,
                    InitiatorId = initiatorId,
                    CreatedAt = DateTime.UtcNow.AddDays(-14 + Array.IndexOf(MockIds.AllThreadIds, id)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7 + Array.IndexOf(MockIds.AllThreadIds, id)),
                    IsActive = true
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
        }

        // Seed Thread Participants (each thread has 2+ participants)
        var threadParticipants = new (Guid threadId, Guid userId)[]
        {
            (MockIds.Thread1, MockIds.DoctorUser1),
            (MockIds.Thread1, MockIds.PatientUser1),
            (MockIds.Thread1, MockIds.DoctorUser2), // Care team includes multiple doctors
            (MockIds.Thread2, MockIds.DoctorUser5),
            (MockIds.Thread2, MockIds.PatientUser5),
            (MockIds.Thread3, MockIds.DoctorUser3),
            (MockIds.Thread3, MockIds.PatientUser3),
            (MockIds.Thread4, MockIds.PatientUser4),
            (MockIds.Thread4, MockIds.DoctorUser4),
            (MockIds.Thread5, MockIds.PatientUser2),
            (MockIds.Thread5, MockIds.DoctorUser2),
            (MockIds.Thread6, MockIds.PatientUser6),
            (MockIds.Thread6, MockIds.DoctorUser6),
            (MockIds.Thread7, MockIds.DoctorUser7),
            (MockIds.Thread7, MockIds.PatientUser7)
        };

        var existingParticipants = await db.ThreadParticipants
            .Select(p => new { p.ThreadId, p.UserId })
            .ToListAsync();
        var existingParticipantSet = existingParticipants.Select(x => (x.ThreadId, x.UserId)).ToHashSet();

        foreach (var (threadId, userId) in threadParticipants)
        {
            if (!existingParticipantSet.Contains((threadId, userId)))
            {
                db.ThreadParticipants.Add(new ThreadParticipant
                {
                    Id = Guid.NewGuid(),
                    ThreadId = threadId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow.AddDays(-14),
                    IsActive = true
                });
                created++;
            }
        }

        // Seed Thread Messages (messages within threads)
        var threadMessageData = new (Guid threadId, Guid senderId, string content)[]
        {
            (MockIds.Thread1, MockIds.DoctorUser1, "Team, Alice is making good progress with her treatment plan."),
            (MockIds.Thread1, MockIds.DoctorUser2, "Agreed. Her vitals have been stable. Recommend continuing current medication."),
            (MockIds.Thread1, MockIds.PatientUser1, "Thank you both for the excellent care!"),
            (MockIds.Thread2, MockIds.DoctorUser5, "Your post-surgery recovery is going well. Continue with physical therapy."),
            (MockIds.Thread2, MockIds.PatientUser5, "The pain is much better now. Thank you!"),
            (MockIds.Thread3, MockIds.DoctorUser3, "Your blood work results look excellent. Keep up the healthy lifestyle."),
            (MockIds.Thread3, MockIds.PatientUser3, "That's great news! Should I continue the same diet?"),
            (MockIds.Thread4, MockIds.PatientUser4, "Hello, I need to renew my child's prescription."),
            (MockIds.Thread4, MockIds.DoctorUser4, "I've sent the renewal to your pharmacy. It should be ready tomorrow."),
            (MockIds.Thread5, MockIds.PatientUser2, "Can we reschedule to next Friday at 2pm?"),
            (MockIds.Thread5, MockIds.DoctorUser2, "That works for me. I've updated your appointment.")
        };

        var existingThreadMsgCount = await db.ThreadMessages.CountAsync();
        if (existingThreadMsgCount < threadMessageData.Length)
        {
            foreach (var (threadId, senderId, content) in threadMessageData)
            {
                var exists = await db.ThreadMessages.AnyAsync(tm => tm.ThreadId == threadId && tm.Content == content);
                if (!exists)
                {
                    db.ThreadMessages.Add(new ThreadMessage
                    {
                        Id = Guid.NewGuid(),
                        ThreadId = threadId,
                        SenderId = senderId,
                        Content = content,
                        SentAt = DateTime.UtcNow.AddDays(-7 + threadMessageData.ToList().IndexOf((threadId, senderId, content)) % 7),
                        CreatedAt = DateTime.UtcNow.AddDays(-7)
                    });
                    created++;
                }
            }
        }

        // Seed Message Receipts (read receipts for messages)
        var receiptsData = new (Guid messageId, Guid userId)[]
        {
            (MockIds.Message1, MockIds.DoctorUser1),
            (MockIds.Message2, MockIds.PatientUser1),
            (MockIds.Message4, MockIds.PatientUser3),
            (MockIds.Message6, MockIds.PatientUser5)
        };

        var existingReceipts = await db.MessageReceipts
            .Select(r => new { r.MessageId, r.UserId })
            .ToListAsync();
        var existingReceiptSet = existingReceipts.Select(x => (x.MessageId, x.UserId)).ToHashSet();

        foreach (var (messageId, userId) in receiptsData)
        {
            if (!existingReceiptSet.Contains((messageId, userId)))
            {
                db.MessageReceipts.Add(new MessageReceipt
                {
                    Id = Guid.NewGuid(),
                    MessageId = messageId,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                });
                created++;
            }
        }

        if (created > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {created} messaging records (messages, threads, participants, thread messages, receipts).");
        }
        else
        {
            Console.WriteLine("[MockDataSeeder] All messaging mock data already exists.");
        }

        // Seed PatientDoctorContacts (for message recipient lookup)
        // This maps which doctors a patient can message (based on appointments)
        var patientDoctorContactData = new (Guid patientUserId, Guid doctorUserId, string doctorName, string specialization)[]
        {
            // Mock doctor names matching PractitionerService mock data
            (MockIds.PatientUser1, MockIds.DoctorUser1, "Dr. John Carter", "Internal Medicine"),
            (MockIds.PatientUser1, MockIds.DoctorUser2, "Dr. Sarah Chen", "Cardiology"),
            (MockIds.PatientUser2, MockIds.DoctorUser2, "Dr. Sarah Chen", "Cardiology"),
            (MockIds.PatientUser2, MockIds.DoctorUser3, "Dr. Michael Brown", "Dermatology"),
            (MockIds.PatientUser3, MockIds.DoctorUser3, "Dr. Michael Brown", "Dermatology"),
            (MockIds.PatientUser3, MockIds.DoctorUser1, "Dr. John Carter", "Internal Medicine"),
            (MockIds.PatientUser4, MockIds.DoctorUser4, "Dr. Emily Thompson", "Pediatrics"),
            (MockIds.PatientUser5, MockIds.DoctorUser5, "Dr. David Wilson", "Orthopedics"),
            (MockIds.PatientUser6, MockIds.DoctorUser6, "Dr. Lisa Anderson", "Neurology"),
            (MockIds.PatientUser7, MockIds.DoctorUser7, "Dr. Robert Martinez", "Psychiatry"),
        };

        var existingContacts = await db.PatientDoctorContacts
            .Select(c => new { c.PatientUserId, c.DoctorUserId })
            .ToListAsync();
        var existingContactSet = existingContacts.Select(x => (x.PatientUserId, x.DoctorUserId)).ToHashSet();

        int contactsCreated = 0;
        foreach (var (patientUserId, doctorUserId, doctorName, specialization) in patientDoctorContactData)
        {
            if (!existingContactSet.Contains((patientUserId, doctorUserId)))
            {
                var now = DateTime.UtcNow;
                db.PatientDoctorContacts.Add(new PatientDoctorContact
                {
                    Id = Guid.NewGuid(),
                    PatientUserId = patientUserId,
                    DoctorUserId = doctorUserId,
                    DoctorName = doctorName,
                    DoctorSpecialization = specialization,
                    FirstContactAt = now.AddDays(-30),
                    LastContactAt = now.AddDays(-1),
                    CreatedAt = now,
                    UpdatedAt = now
                });
                contactsCreated++;
            }
        }

        if (contactsCreated > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"[MockDataSeeder] Created {contactsCreated} PatientDoctorContact records.");
        }

        // Update existing contacts that have empty/generic names
        // Query User_Profile table to get real names
        try
        {
            var contactsNeedingUpdate = await db.PatientDoctorContacts
                .Where(c => c.DoctorName == null || c.DoctorName == "Doctor" || c.DoctorName == "")
                .ToListAsync();

            if (contactsNeedingUpdate.Any())
            {
                Console.WriteLine($"[MockDataSeeder] Found {contactsNeedingUpdate.Count} contacts needing name update.");
                
                foreach (var contact in contactsNeedingUpdate)
                {
                    try
                    {
                        // Query shared user.User_Profile table
                        var userProfile = await db.Database.SqlQueryRaw<UserProfileQueryResult>(
                            "SELECT FirstName, LastName FROM [user].[User_Profile] WHERE Id = {0}",
                            contact.DoctorUserId)
                            .FirstOrDefaultAsync();

                        if (userProfile != null && (!string.IsNullOrEmpty(userProfile.FirstName) || !string.IsNullOrEmpty(userProfile.LastName)))
                        {
                            contact.DoctorName = $"Dr. {userProfile.FirstName} {userProfile.LastName}".Trim();
                            contact.UpdatedAt = DateTime.UtcNow;
                            Console.WriteLine($"[MockDataSeeder] Updated contact for doctor {contact.DoctorUserId}: {contact.DoctorName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MockDataSeeder] Failed to fetch name for {contact.DoctorUserId}: {ex.Message}");
                    }
                }

                await db.SaveChangesAsync();
                Console.WriteLine("[MockDataSeeder] Finished updating contact names.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MockDataSeeder] Error updating contact names: {ex.Message}");
        }
    }
}

// DTO for SQL query result
public record UserProfileQueryResult(string? FirstName, string? LastName);
