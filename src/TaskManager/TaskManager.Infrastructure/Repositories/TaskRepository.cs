using Microsoft.Data.Sqlite;
using TaskManager.Domain.Entities;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;
using TaskManager.Domain.Repositories;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TaskRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Status, DueDate, UserId, CreatedAt, UpdatedAt
            FROM Tasks
            WHERE Id = @Id
            """;
        command.Parameters.Add(new SqliteParameter("@Id", id.ToString()));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapTask(reader);
    }

    public async Task<IReadOnlyCollection<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = new List<TaskItem>();

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Status, DueDate, UserId, CreatedAt, UpdatedAt
            FROM Tasks
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC
            """;
        command.Parameters.Add(new SqliteParameter("@UserId", userId.ToString()));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(MapTask(reader));
        }

        return result;
    }

    public async Task<TaskItem> CreateAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Tasks (Id, Title, Description, Status, DueDate, UserId, CreatedAt, UpdatedAt)
            VALUES (@Id, @Title, @Description, @Status, @DueDate, @UserId, @CreatedAt, @UpdatedAt)
            """;
        command.Parameters.Add(new SqliteParameter("@Id", taskItem.Id.ToString()));
        command.Parameters.Add(new SqliteParameter("@Title", taskItem.Title));
        command.Parameters.Add(new SqliteParameter("@Description", taskItem.Description));
        command.Parameters.Add(new SqliteParameter("@Status", taskItem.Status.ToString()));
        command.Parameters.Add(new SqliteParameter("@DueDate", taskItem.DueDate?.ToString("O") ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@UserId", taskItem.UserId.ToString()));
        command.Parameters.Add(new SqliteParameter("@CreatedAt", taskItem.CreatedAt.ToString("O")));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", taskItem.UpdatedAt.ToString("O")));

        await command.ExecuteNonQueryAsync(cancellationToken);
        return taskItem;
    }

    public async Task<bool> UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Tasks
            SET Title = @Title,
                Description = @Description,
                Status = @Status,
                DueDate = @DueDate,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        command.Parameters.Add(new SqliteParameter("@Id", taskItem.Id.ToString()));
        command.Parameters.Add(new SqliteParameter("@Title", taskItem.Title));
        command.Parameters.Add(new SqliteParameter("@Description", taskItem.Description));
        command.Parameters.Add(new SqliteParameter("@Status", taskItem.Status.ToString()));
        command.Parameters.Add(new SqliteParameter("@DueDate", taskItem.DueDate?.ToString("O") ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", taskItem.UpdatedAt.ToString("O")));

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tasks WHERE Id = @Id";
        command.Parameters.Add(new SqliteParameter("@Id", id.ToString()));

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    private static TaskItem MapTask(SqliteDataReader reader)
    {
        return new TaskItem
        {
            Id = Guid.Parse(reader.GetString(0)),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Status = Enum.TryParse<DomainTaskStatus>(reader.GetString(3), true, out var status) ? status : DomainTaskStatus.Pending,
            DueDate = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
            UserId = Guid.Parse(reader.GetString(5)),
            CreatedAt = DateTime.Parse(reader.GetString(6)),
            UpdatedAt = DateTime.Parse(reader.GetString(7))
        };
    }
}
