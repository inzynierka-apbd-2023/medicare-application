namespace PdfService.Services;

public interface IPdfGenerator
{
    byte[] BuildPdf(Dictionary<string, object?> data);
}
