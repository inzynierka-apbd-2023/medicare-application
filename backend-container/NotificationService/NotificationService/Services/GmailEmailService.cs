using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace NotificationService.Services;

public class GmailEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<GmailEmailService> _logger;
    private readonly string _frontendBaseUrl;

    public GmailEmailService(IOptions<SmtpSettings> settings, ILogger<GmailEmailService> logger, IConfiguration config)
    {
        _settings = settings.Value;
        _logger = logger;
        _frontendBaseUrl = config["FrontendBaseUrl"] ?? "http://localhost:3000";
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken ct = default)
    {
        var subject = "Welcome to Medicare!";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #3b82f6, #1d4ed8); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #3b82f6; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Medicare!</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName},</h2>
            <p>Thank you for registering with Medicare. Your account has been successfully created.</p>
            <p>You can now:</p>
            <ul>
                <li>Book appointments with our doctors</li>
                <li>View your medical records</li>
                <li>Manage your prescriptions</li>
                <li>And much more!</li>
            </ul>
            <p style='text-align: center;'>
                <a href='{_frontendBaseUrl}/login' class='button'>Sign In Now</a>
            </p>
            <p>If you have any questions, please don't hesitate to contact our support team.</p>
            <p>Best regards,<br/>The Medicare Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Medicare. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody, ct);
        _logger.LogInformation("Welcome email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken, CancellationToken ct = default)
    {
        var resetUrl = $"{_frontendBaseUrl}/reset-password?token={resetToken}";
        var subject = "Reset Your Medicare Password";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #3b82f6, #1d4ed8); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #3b82f6; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; margin: 20px 0; }}
        .warning {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hello {firstName},</h2>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Reset Password</a>
            </p>
            <div class='warning'>
                <strong>Important:</strong> This link will expire in 1 hour. If you didn't request a password reset, please ignore this email.
            </div>
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #3b82f6;'>{resetUrl}</p>
            <p>Best regards,<br/>The Medicare Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Medicare. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody, ct);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "Medicare";
}
