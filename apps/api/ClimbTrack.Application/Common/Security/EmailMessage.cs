namespace ClimbTrack.Application.Common.Security;

public record EmailMessage(
    string ToEmail,
    string ToName,
    string Subject,
    string PlainTextBody,
    string? HtmlBody = null);
