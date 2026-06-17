namespace ClimbTrack.Application.Common.Security;

public interface IPasswordResetTokenService
{
    PasswordResetTokenData CreateToken();
    string HashToken(string token);
}
