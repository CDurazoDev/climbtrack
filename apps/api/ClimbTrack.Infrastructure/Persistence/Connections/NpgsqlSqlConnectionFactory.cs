using System.Data.Common;
using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Domain.Interfaces;
using Npgsql;

namespace ClimbTrack.Infrastructure.Persistence.Connections;

public class NpgsqlSqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;
    private readonly ICurrentUserService _currentUser;

    public NpgsqlSqlConnectionFactory(string connectionString, ICurrentUserService currentUser)
    {
        _connectionString = connectionString;
        _currentUser = currentUser;
    }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (_currentUser.UserId.HasValue)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "select set_config('app.current_user_id', @userId, false);";
            command.Parameters.AddWithValue("userId", _currentUser.UserId.Value.ToString());
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return connection;
    }
}
