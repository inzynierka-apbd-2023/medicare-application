using PdfService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PdfService.Workers;

public sealed class PdfGenerationWorker : BackgroundService
{
    private readonly ILogger<PdfGenerationWorker> _logger;
    private readonly IConnection _connection;
    private readonly IPdfGenerator _pdfGenerator;
    private IChannel? _channel;
    
    private const string RequestQueue = "pdf.generate.document";

    public PdfGenerationWorker(ILogger<PdfGenerationWorker> logger, IConnection connection, IPdfGenerator pdfGenerator)
    {
        _logger = logger;
        _connection = connection;
        _pdfGenerator = pdfGenerator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {        
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        await _channel.QueueDeclareAsync(
            queue: RequestQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);
        
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            await ProcessMessageAsync(ea, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: RequestQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken ct)
    {
        var replyTo = ea.BasicProperties.ReplyTo;
        var corrId = ea.BasicProperties.CorrelationId;
        
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
        var payload = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (payload == null)
        {
            _logger.LogWarning("[PdfService] Received null payload for CorrelationId={CorrelationId}", corrId);
            await AckMessageAsync(ea);
            return;
        }
        
        // Use the generator service
        byte[] pdf;
        try 
        {
            pdf = _pdfGenerator.BuildPdf(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PdfService] Failed to generate PDF for CorrelationId={CorrelationId}", corrId);
            await AckMessageAsync(ea);
            return;
        }

        if (!string.IsNullOrWhiteSpace(replyTo) && !string.IsNullOrWhiteSpace(corrId) && _channel != null)
        {
            var props = new BasicProperties();
            props.CorrelationId = corrId;
            props.ContentType = "application/pdf";
            
            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: replyTo,
                mandatory: false,
                basicProperties: props,
                body: pdf,
                cancellationToken: ct);
            
            _logger.LogInformation("[PdfService] Published response CorrelationId={CorrelationId}, SizeBytes={Size}", corrId, pdf.Length);
        }
        
        await AckMessageAsync(ea);
    }

    private async Task AckMessageAsync(BasicDeliverEventArgs ea)
    {
        if (_channel != null)
        {
            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {        
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }
        
        await base.StopAsync(cancellationToken);
    }
}
