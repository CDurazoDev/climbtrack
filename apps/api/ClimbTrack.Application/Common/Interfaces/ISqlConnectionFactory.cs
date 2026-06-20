using System.Data.Common;

namespace ClimbTrack.Application.Common.Interfaces;

public interface ISqlConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
