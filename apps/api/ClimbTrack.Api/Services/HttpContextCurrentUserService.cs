using System.Security.Claims;
using ClimbTrack.Domain.Interfaces;

namespace ClimbTrack.Api.Services;

public class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var userIdValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }

    public string? Role => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
}
