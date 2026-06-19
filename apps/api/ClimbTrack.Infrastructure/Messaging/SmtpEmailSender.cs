using System.Net;
using System.Net.Mail;
using ClimbTrack.Application.Common.Security;
using Microsoft.Extensions.Configuration;

namespace ClimbTrack.Infrastructure.Messaging;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var host = GetRequiredValue("Email:Smtp:Host");
        var port = int.TryParse(_configuration["Email:Smtp:Port"], out var configuredPort)
            ? configuredPort
            : 587;
        var username = GetRequiredValue("Email:Smtp:Username");
        var password = GetRequiredValue("Email:Smtp:Password");
        var enableSsl = bool.TryParse(_configuration["Email:Smtp:EnableSsl"], out var configuredEnableSsl)
            ? configuredEnableSsl
            : true;
        var fromAddress = GetRequiredValue("Email:FromAddress");
        var fromName = _configuration["Email:FromName"] ?? "ClimbTrack";

        using var mail = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = message.Subject,
            Body = message.HtmlBody ?? message.PlainTextBody,
            IsBodyHtml = !string.IsNullOrWhiteSpace(message.HtmlBody),
        };

        mail.To.Add(new MailAddress(message.ToEmail, message.ToName));

        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            mail.AlternateViews.Add(
                AlternateView.CreateAlternateViewFromString(message.PlainTextBody, null, "text/plain"));
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(username, password),
        };

        await client.SendMailAsync(mail, cancellationToken);
    }

    private string GetRequiredValue(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value) || value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"{key} must be configured from a secure local source.");
        }

        return value;
    }
}
