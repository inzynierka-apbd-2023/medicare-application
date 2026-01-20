using MassTransit;
using Medicare.Messaging.Contracts;
using PdfService.Services;
using System.Text.Json;

namespace PdfService.Messaging.Consumers;

public class PdfGenerationConsumer : IConsumer<IGeneratePdfRequest>
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ILogger<PdfGenerationConsumer> _logger;

    public PdfGenerationConsumer(IPdfGenerator pdfGenerator, ILogger<PdfGenerationConsumer> logger)
    {
        _pdfGenerator = pdfGenerator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IGeneratePdfRequest> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received PDF generation request for DocumentId={DocumentId}", msg.DocumentId);

        Dictionary<string, object?>? payload = null;
        payload = JsonSerializer.Deserialize<Dictionary<string, object?>>(msg.PayloadJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload == null)
        {
             _logger.LogError("Payload is null for DocumentId={DocumentId}", msg.DocumentId);
             throw new InvalidOperationException("Payload cannot be null");
        }

        byte[] pdfBytes;
        try
        {
            pdfBytes = _pdfGenerator.BuildPdf(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for DocumentId={DocumentId}", msg.DocumentId);
            throw;
        }

        await context.RespondAsync<IPdfGeneratedResponse>(new 
        { 
            DocumentId = msg.DocumentId, 
            PdfBytes = pdfBytes 
        });
        
        _logger.LogInformation("PDF generated and response sent for DocumentId={DocumentId}, Size={Size}", msg.DocumentId, pdfBytes.Length);
    }
}
