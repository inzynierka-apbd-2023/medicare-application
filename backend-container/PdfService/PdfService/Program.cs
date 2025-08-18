using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PdfService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(b => b.AddConsole())
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("PdfService");

        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ__HOST") ?? "rabbitmq";
        var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ__USERNAME") ?? "guest";
        var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ__PASSWORD") ?? "guest";

        var factory = new ConnectionFactory { HostName = rabbitHost, UserName = rabbitUser, Password = rabbitPass, DispatchConsumersAsync = true };
        using var conn = factory.CreateConnection();
        using var channel = conn.CreateModel();
        var queue = "pdf.generate.document";
        channel.QueueDeclare(queue, durable: false, exclusive: false, autoDelete: false);
        channel.BasicQos(0, 1, false);

        QuestPDF.Settings.License = LicenseType.Community;

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (ch, ea) =>
        {
            try
            {
                var replyTo = ea.BasicProperties?.ReplyTo;
                var corrId = ea.BasicProperties?.CorrelationId;
                logger.LogInformation("[PdfService] Received request CorrelationId={CorrelationId}, ReplyTo={ReplyTo}", corrId, replyTo);
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var payload = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var pdf = BuildPdf(payload!);

                if (!string.IsNullOrWhiteSpace(replyTo) && !string.IsNullOrWhiteSpace(corrId))
                {
                    var props = channel.CreateBasicProperties();
                    props.CorrelationId = corrId;
                    props.ContentType = "application/pdf";
                    channel.BasicPublish(exchange: "", routingKey: replyTo, basicProperties: props, body: pdf);
                    logger.LogInformation("[PdfService] Published response CorrelationId={CorrelationId}, SizeBytes={Size}", corrId, pdf?.Length ?? 0);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate PDF");
            }
            finally
            {
                channel.BasicAck(ea.DeliveryTag, multiple: false);
                await Task.CompletedTask;
            }
        };
        channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);

        logger.LogInformation("PdfService is running and listening on {Queue}", queue);
        await host.RunAsync();
    }

    private static byte[] BuildPdf(Dictionary<string, object?> data)
    {
        var type = data.GetValueOrDefault("Type")?.ToString() ?? "Document";
        var docId = data.GetValueOrDefault("DocumentId")?.ToString();
        var createdAt = data.GetValueOrDefault("CreatedAt")?.ToString();
    var patientId = data.GetValueOrDefault("PatientId")?.ToString();
    var doctorId = data.GetValueOrDefault("DoctorId")?.ToString();
    var patientName = data.GetValueOrDefault("PatientName")?.ToString();
    var doctorName = data.GetValueOrDefault("DoctorName")?.ToString();
        var notes = data.GetValueOrDefault("Notes")?.ToString();
    const string SignatureLabel = "Physician Signature: ______________________________";
    // Unified accent color (prescription blue) for all document types
    string accent = Colors.Blue.Medium;

    var bytes = Document.Create(c =>
        {
            c.Page(p =>
            {
                p.Margin(40);
                p.Size(PageSizes.A4);
                p.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));
                p.Header().Column(header =>
                {
                    header.Item().Element(e =>
                        e.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(6)
                         .Row(r =>
                         {
                             r.AutoItem().Element(b => b.Width(6).Height(36).Background(accent));
                             r.RelativeItem().PaddingLeft(10).Column(col =>
                             {
                                 col.Item().Text("Medicare Clinic").Bold().FontSize(18);
                                 col.Item().Text("123 Health St, Wellness City • (555) 123-4567").FontSize(10).FontColor(Colors.Grey.Darken1);
                             });
                             r.AutoItem().AlignRight().Text(type.Replace('_',' ')).Bold().FontSize(14).FontColor(accent);
                         })
                    );
                });

                p.Content().Column(col =>
                {
                    // Title band and meta information
                    col.Item().Element(e =>
                    {
                        e.Background(Colors.Grey.Lighten5).BorderLeft(4).BorderColor(accent).Padding(10).Row(r =>
                        {
                            r.RelativeItem().Column(c2 =>
                            {
                                c2.Item().Text(type.Replace('_',' ')).Bold().FontSize(20).FontColor(accent);
                                if (!string.IsNullOrWhiteSpace(notes))
                                    c2.Item().Text(notes!).FontColor(Colors.Grey.Darken1);
                            });
                            r.AutoItem().Element(b =>
                            {
                                b.PaddingHorizontal(6).PaddingVertical(2).Background(accent)
                                    .DefaultTextStyle(s => s.FontColor(Colors.White))
                                    .Text(type switch
                                    {
                                        "Prescription" => "Rx",
                                        "Referral" => "Referral",
                                        "SickLeave" => "Sick Leave",
                                        "VisitNote" => "Visit",
                                        "LabResults" => "Labs",
                                        _ => "Document"
                                    });
                            });
                        });
                    });

                    col.Item().PaddingTop(10).Row(r =>
                    {
                        r.RelativeItem().Column(ci =>
                        {
                            var patientDisplay = string.IsNullOrWhiteSpace(patientName) ? (patientId ?? "-") : patientName!;
                            var doctorDisplay = string.IsNullOrWhiteSpace(doctorName) ? (doctorId ?? "-") : $"Doctor {doctorName}";
                            ci.Item().Text(t => { t.Span("Patient: ").SemiBold(); t.Span(patientDisplay); });
                            ci.Item().Text(t => { t.Span("Provider: ").SemiBold(); t.Span(doctorDisplay); });
                        });
                        r.AutoItem().Column(ci =>
                        {
                            ci.Item().Text(t => { t.Span("Document ID: ").SemiBold(); t.Span(docId ?? "-"); });
                            ci.Item().Text(t => { t.Span("Issued: ").SemiBold(); t.Span(createdAt ?? "-"); });
                        });
                    });

                    col.Item().PaddingTop(6).Element(e => e.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)).Height(1);

                    // Lab Results template (optional extension)
                    if (type == "LabResults" && data.TryGetValue("LabResults", out var labObj) && labObj is JsonElement labEl)
                    {
                        var labDoc = labEl.Deserialize<LabResultsDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        col.Item().PaddingTop(12).Element(card =>
                        {
                            card.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c3 =>
                            {
                                c3.Item().Text("Laboratory Results").Bold().FontSize(14).FontColor(accent);
                                c3.Item().PaddingTop(4).Row(r =>
                                {
                                    r.RelativeItem().Text($"Test Type: {labDoc?.TestType ?? "-"}");
                                    r.AutoItem().Text($"Test Date: {labDoc?.TestDate?.ToString() ?? "-"}");
                                });
                                c3.Item().Text($"Laboratory: {labDoc?.Laboratory ?? "-"}");
                                if (!string.IsNullOrWhiteSpace(labDoc?.Interpretation))
                                    c3.Item().Text($"Interpretation: {labDoc?.Interpretation}");
                            });
                        });

                        if (labDoc?.Results != null && labDoc.Results.Count > 0)
                        {
                            col.Item().PaddingTop(8).Table(t =>
                            {
                                t.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3); // Parameter
                                    cols.RelativeColumn(2); // Value
                                    cols.RelativeColumn(2); // Unit
                                    cols.RelativeColumn(3); // Reference
                                    cols.RelativeColumn(2); // Status
                                });

                                // Header row
                                void HeaderCell(string text) => t.Cell().Element(e => e.Background(Colors.Grey.Lighten3).Padding(3).Text(text).SemiBold());
                                HeaderCell("Parameter"); HeaderCell("Value"); HeaderCell("Unit"); HeaderCell("Reference"); HeaderCell("Status");

                                foreach (var r in labDoc.Results)
                                {
                                    t.Cell().Text(r.Parameter ?? "-");
                                    t.Cell().Text(r.Value ?? "-");
                                    t.Cell().Text(r.Unit ?? "-");
                                    t.Cell().Text(r.ReferenceRange ?? "-");
                                    var status = r.Status ?? "-";
                                    t.Cell().Element(e =>
                                    {
                                        string color;
                                        if (status.Equals("Critical", StringComparison.OrdinalIgnoreCase))
                                            color = Colors.Red.Medium;
                                        else if (status.Equals("Abnormal", StringComparison.OrdinalIgnoreCase) || status.Equals("High", StringComparison.OrdinalIgnoreCase) || status.Equals("Low", StringComparison.OrdinalIgnoreCase))
                                            color = Colors.Orange.Medium;
                                        else
                                            color = Colors.Green.Darken2;
                                        e.Text(status).FontColor(color).SemiBold();
                                    });
                                }
                            });
                        }
                    }

                    if (type == "Prescription" && data.TryGetValue("Prescription", out var rxObj) && rxObj is JsonElement rxEl)
                    {
                        var rx = rxEl.Deserialize<PrescriptionDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        col.Item().PaddingTop(12).Element(card =>
                        {
                            card.BorderLeft(3).BorderColor(accent).Padding(10).Column(c3 =>
                            {
                                c3.Item().Text("Prescription Details").Bold().FontColor(accent);
                                c3.Item().PaddingTop(4).Row(r =>
                                {
                                    r.RelativeItem().Text(x => { x.Span("Medication: ").SemiBold(); x.Span(rx?.Medication ?? "-"); });
                                    r.AutoItem().Text(x => { x.Span("ATC: ").SemiBold(); x.Span(rx?.AtcCode ?? "-"); });
                                });
                                c3.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(x => { x.Span("Dosage: ").SemiBold(); x.Span(rx?.Dosage ?? "-"); });
                                    r.AutoItem().Text(x => { x.Span("Frequency: ").SemiBold(); x.Span(rx?.Frequency ?? "-"); });
                                });
                                c3.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(x => { x.Span("Duration (days): ").SemiBold(); x.Span(rx?.DurationDays?.ToString() ?? "-"); });
                                    r.AutoItem().Text(x => { x.Span("ATC Name: ").SemiBold(); x.Span(rx?.AtcName ?? "-"); });
                                });
                                c3.Item().Text(x => { x.Span("Instructions: ").SemiBold(); x.Span(rx?.Instructions ?? "-"); });
                            });
                        });
                        col.Item().PaddingTop(16).Row(r => { r.RelativeItem().Text(SignatureLabel); r.AutoItem().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}"); });
                    }
                    else if (type == "Referral" && data.TryGetValue("Referral", out var refObj) && refObj is JsonElement refEl)
                    {
                        var rv = refEl.Deserialize<ReferralDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        col.Item().PaddingTop(12).Element(card =>
                        {
                            card.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c3 =>
                            {
                                c3.Item().Text("Referral Details").Bold().FontColor(accent);
                                c3.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(x => { x.Span("Specialty: ").SemiBold(); x.Span(rv?.Speciality ?? "-"); });
                                    r.AutoItem().Text(x => { x.Span("Urgency: ").SemiBold(); x.Span(rv?.UrgencyLevel ?? "-"); });
                                });
                                c3.Item().Text(x => { x.Span("Referred To: ").SemiBold(); x.Span(rv?.ReferredTo ?? "-"); });
                                c3.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(x => { x.Span("Valid From: ").SemiBold(); x.Span(rv?.ValidFrom?.ToString() ?? "-"); });
                                    r.AutoItem().Text(x => { x.Span("Valid To: ").SemiBold(); x.Span(rv?.ValidTo?.ToString() ?? "-"); });
                                });
                                c3.Item().Text(x => { x.Span("Reason: ").SemiBold(); x.Span(rv?.Reason ?? "-"); });
                            });
                        });
                        col.Item().PaddingTop(16).Row(r => { r.RelativeItem().Text(SignatureLabel); r.AutoItem().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}"); });
                    }
                    else if (type == "SickLeave" && data.TryGetValue("SickLeave", out var slObj) && slObj is JsonElement slEl)
                    {
                        var sl = slEl.Deserialize<SickLeaveDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        col.Item().PaddingTop(12).Element(card =>
                        {
                            card.BorderLeft(3).BorderColor(accent).Padding(10).Column(c3 =>
                            {
                                c3.Item().Text("Sick Leave").Bold().FontColor(accent);
                                c3.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(x => { x.Span("Start Date: ").SemiBold(); x.Span(sl?.StartDate?.ToString() ?? "-"); });
                                    r.AutoItem().Text(x => { x.Span("End Date: ").SemiBold(); x.Span(sl?.EndDate?.ToString() ?? "-"); });
                                });
                                c3.Item().Text(x => { x.Span("Days Off: ").SemiBold(); x.Span(sl?.DaysOff?.ToString() ?? "-"); });
                                c3.Item().Text(x => { x.Span("Restrictions: ").SemiBold(); x.Span(sl?.WorkRestrictions ?? "-"); });
                            });
                        });
                        col.Item().PaddingTop(16).Row(r => { r.RelativeItem().Text(SignatureLabel); r.AutoItem().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}"); });
                    }
                    else if (type == "VisitNote" && data.TryGetValue("Visit", out var vObj) && vObj is JsonElement vEl)
                    {
                        var vn = vEl.Deserialize<VisitDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        col.Item().PaddingTop(12).Element(card =>
                        {
                            card.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c3 =>
                            {
                                c3.Item().Text("Visit Summary (SOAP)").Bold().FontColor(accent);
                                c3.Item().PaddingTop(4).Column(s =>
                                {
                                    s.Item().Text("Subjective").SemiBold();
                                    s.Item().Text(vn?.Symptoms ?? "-").FontColor(Colors.Grey.Darken1);
                                    s.Item().PaddingTop(6).Text("Objective / Findings").SemiBold();
                                    s.Item().Text(vn?.Findings ?? "-").FontColor(Colors.Grey.Darken1);
                                    s.Item().PaddingTop(6).Text("Assessment (Diagnosis)").SemiBold();
                                    s.Item().Text(vn?.Diagnosis ?? "-").FontColor(Colors.Grey.Darken1);
                                    s.Item().PaddingTop(6).Text("Plan (Recommendations)").SemiBold();
                                    s.Item().Text(vn?.Recommendations ?? "-").FontColor(Colors.Grey.Darken1);
                                    s.Item().PaddingTop(6).Text(t => { t.Span("Follow Up: ").SemiBold(); t.Span(vn?.FollowUpDate?.ToString() ?? "-"); });
                                });
                            });
                        });
                        col.Item().PaddingTop(16).Row(r => { r.RelativeItem().Text(SignatureLabel); r.AutoItem().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}"); });
                    }

                    col.Item().PaddingTop(20).Text("This document was generated electronically and is valid without a signature.").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                });

                p.Footer().AlignCenter().Text(x =>
                {
                    x.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Darken1));
                    x.Span("Medicare Clinic • ");
                    x.Span("Confidential Medical Document • ");
                    x.Span(DateTime.UtcNow.ToString("u"));
                    x.Span(" • Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();

        return bytes;
    }
}

public sealed record PrescriptionDto(string? Medication, string? Dosage, string? Frequency, int? DurationDays, string? Instructions, string? AtcCode, string? AtcName);
public sealed record ReferralDto(string? Speciality, string? ReferredTo, DateTime? ValidFrom, DateTime? ValidTo, string? Reason, string? UrgencyLevel);
public sealed record SickLeaveDto(string? StartDate, string? EndDate, int? DaysOff, string? WorkRestrictions);
public sealed record VisitDto(string? Symptoms, string? Findings, string? Diagnosis, string? Recommendations, string? FollowUpDate);
public sealed record LabResultsDto(string? TestType, DateTime? TestDate, string? Laboratory, string? Interpretation, List<LabResultItemDto>? Results);
public sealed record LabResultItemDto(string? Parameter, string? Value, string? Unit, string? ReferenceRange, string? Status);
