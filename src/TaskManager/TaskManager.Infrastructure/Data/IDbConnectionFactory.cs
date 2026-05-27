using Microsoft.Data.Sqlite;

namespace TaskManager.Infrastructure.Data;

public interface IDbConnectionFactory
{
    Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
