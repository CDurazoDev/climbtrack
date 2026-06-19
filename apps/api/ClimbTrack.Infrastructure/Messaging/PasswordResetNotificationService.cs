using System.Net;
using ClimbTrack.Application.Common.Security;
using ClimbTrack.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace ClimbTrack.Infrastructure.Messaging;

public class PasswordResetNotificationService : IPasswordResetNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;

    public PasswordResetNotificationService(IConfiguration configuration, IEmailSender emailSender)
    {
        _configuration = configuration;
        _emailSender = emailSender;
    }

    public async Task SendResetInstructionsAsync(User user, string resetToken, CancellationToken cancellationToken)
    {
        var resetUrlTemplate = _configuration["Auth:PasswordResetUrlTemplate"];
        var resetUrl = string.IsNullOrWhiteSpace(resetUrlTemplate)
            ? null
            : resetUrlTemplate.Replace("{token}", WebUtility.UrlEncode(resetToken), StringComparison.Ordinal);
        var lifetimeMinutes = GetLifetimeMinutes();

        var plainTextBody = BuildPlainTextBody(user.Name, resetToken, resetUrl, lifetimeMinutes);
        var htmlBody = BuildHtmlBody(user.Name, resetToken, resetUrl, lifetimeMinutes);

        await _emailSender.SendAsync(
            new EmailMessage(
                user.Email,
                user.Name,
                "ClimbTrack password reset",
                plainTextBody,
                htmlBody),
            cancellationToken);
    }

    private static string BuildPlainTextBody(string name, string resetToken, string? resetUrl, int lifetimeMinutes)
    {
        var lines = new List<string>
        {
            $"Hello {name},",
            "",
            "We received a request to reset your ClimbTrack password.",
            "Use the following token in the app to set a new password:",
            resetToken,
            "",
            $"This token expires in {lifetimeMinutes} minutes and can only be used once.",
        };

        if (!string.IsNullOrWhiteSpace(resetUrl))
        {
            lines.Add("");
            lines.Add("If you have a compatible reset screen, you can also open:");
            lines.Add(resetUrl);
        }

        lines.Add("");
        lines.Add("If you did not request this change, you can ignore this email.");

        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildHtmlBody(string name, string resetToken, string? resetUrl, int lifetimeMinutes)
    {
        var actionLink = string.IsNullOrWhiteSpace(resetUrl)
            ? string.Empty
            : $"""<p><a href="{WebUtility.HtmlEncode(resetUrl)}">Open password reset</a></p>""";

        return $"""
                <p>Hello {WebUtility.HtmlEncode(name)},</p>
                <p>We received a request to reset your ClimbTrack password.</p>
                <p>Use the following token in the app to set a new password:</p>
                <p><strong>{WebUtility.HtmlEncode(resetToken)}</strong></p>
                <p>This token expires in {lifetimeMinutes} minutes and can only be used once.</p>
                {actionLink}
                <p>If you did not request this change, you can ignore this email.</p>
                """;
    }

    private int GetLifetimeMinutes()
    {
        return int.TryParse(_configuration["Auth:PasswordResetTokenLifetimeMinutes"], out var lifetimeMinutes)
            ? lifetimeMinutes
            : 30;
    }
}
