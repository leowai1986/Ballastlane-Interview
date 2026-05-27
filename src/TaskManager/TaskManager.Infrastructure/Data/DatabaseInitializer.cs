using Microsoft.Extensions.Hosting;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IHostEnvironment _hostEnvironment;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory, IHostEnvironment hostEnvironment)
    {
        _connectionFactory = connectionFactory;
        _hostEnvironment = hostEnvironment;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var sqlPath = Path.Combine(_hostEnvironment.ContentRootPath, "Data", "init.sql");
        if (!File.Exists(sqlPath))
        {
            throw new FileNotFoundException("Could not find init.sql", sqlPath);
        }

        var sql = await File.ReadAllTextAsync(sqlPath, cancellationToken);
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
