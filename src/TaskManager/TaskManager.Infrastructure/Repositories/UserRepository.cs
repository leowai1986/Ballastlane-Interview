using Microsoft.Data.Sqlite;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Repositories;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Email, PasswordHash, Name, CreatedAt, UpdatedAt
            FROM Users
            WHERE Id = @Id
            """;
        command.Parameters.Add(new SqliteParameter("@Id", id.ToString()));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Email, PasswordHash, Name, CreatedAt, UpdatedAt
            FROM Users
            WHERE Email = @Email
            """;
        command.Parameters.Add(new SqliteParameter("@Email", email));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Id, Email, PasswordHash, Name, CreatedAt, UpdatedAt)
            VALUES (@Id, @Email, @PasswordHash, @Name, @CreatedAt, @UpdatedAt)
            """;
        command.Parameters.Add(new SqliteParameter("@Id", user.Id.ToString()));
        command.Parameters.Add(new SqliteParameter("@Email", user.Email));
        command.Parameters.Add(new SqliteParameter("@PasswordHash", user.PasswordHash));
        command.Parameters.Add(new SqliteParameter("@Name", user.Name));
        command.Parameters.Add(new SqliteParameter("@CreatedAt", user.CreatedAt.ToString("O")));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", user.UpdatedAt.ToString("O")));

        await command.ExecuteNonQueryAsync(cancellationToken);
        return user;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM Users WHERE Email = @Email LIMIT 1";
        command.Parameters.Add(new SqliteParameter("@Email", email));

        var exists = await command.ExecuteScalarAsync(cancellationToken);
        return exists is not null;
    }

    private static User MapUser(SqliteDataReader reader)
    {
        return new User
        {
            Id = Guid.Parse(reader.GetString(0)),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            Name = reader.GetString(3),
            CreatedAt = DateTime.Parse(reader.GetString(4)),
            UpdatedAt = DateTime.Parse(reader.GetString(5))
        };
    }
}
