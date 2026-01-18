namespace NotificationService.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken, CancellationToken ct = default);
}
