using ClimbTrack.Domain.Entities;

namespace ClimbTrack.Application.Common.Security;

public interface IPasswordResetNotificationService
{
    Task SendResetInstructionsAsync(User user, string resetToken, CancellationToken cancellationToken);
}
