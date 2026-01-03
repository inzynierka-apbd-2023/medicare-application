namespace NotificationService.Services;

/// <summary>
/// Email service interface for sending transactional emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send a welcome email to a newly registered user
    /// </summary>
    Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken ct = default);

    /// <summary>
    /// Send a password reset email with reset link
    /// </summary>
    Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken, CancellationToken ct = default);
}
