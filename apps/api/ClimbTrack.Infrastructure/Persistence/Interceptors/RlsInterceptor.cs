using System.Data.Common;
using ClimbTrack.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClimbTrack.Infrastructure.Persistence.Interceptors;

public class RlsInterceptor : DbConnectionInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public RlsInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId.HasValue)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SET app.current_user_id = '{_currentUser.UserId.Value}'";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}

