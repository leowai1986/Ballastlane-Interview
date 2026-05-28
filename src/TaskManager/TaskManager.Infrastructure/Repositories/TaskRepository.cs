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

    public async Task<List<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Description, Status, DueDate, UserId, CreatedAt, UpdatedAt
            FROM Tasks
            WHERE UserId = @UserId
            """;
        command.Parameters.Add(new SqliteParameter("@UserId", userId.ToString()));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var tasks = new List<TaskItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public async Task<PagedResult<TaskItem>> GetByUserIdPaginatedAsync(Guid userId,
                                                                TaskFilterRequest filter,
                                                                CancellationToken cancellationToken = default)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 1;
        if (filter.PageSize > 100) filter.PageSize = 100;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var countSql = """
            SELECT COUNT(*)
            FROM Tasks
            WHERE UserId = @UserId
            """;

        var whereConditions = new List<string>();
        var parameters = new List<SqliteParameter>
        {
            new("@UserId", userId.ToString())
        };

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            whereConditions.Add("Status = @Status");
            parameters.Add(new("@Status", filter.Status));
        }

        if (filter.DueDateFrom.HasValue)
        {
            whereConditions.Add("DueDate >= @DueDateFrom");
            parameters.Add(new("@DueDateFrom", filter.DueDateFrom.Value));
        }

        if (filter.DueDateTo.HasValue)
        {
            whereConditions.Add("DueDate <= @DueDateTo");
            parameters.Add(new("@DueDateTo", filter.DueDateTo.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            whereConditions.Add("(Title LIKE @SearchTerm OR Description LIKE @SearchTerm)");
            parameters.Add(new("@SearchTerm", $"%{filter.SearchTerm}%"));
        }

        if (whereConditions.Count > 0)
        {
            countSql += " AND " + string.Join(" AND ", whereConditions);
        }

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = countSql;
        foreach (var param in parameters) countCommand.Parameters.Add(param);

        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

        var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Id", "Title", "Description", "Status", "DueDate", "CreatedAt", "UpdatedAt"
        };

        var sortColumn = allowedSortColumns.Contains(filter.SortBy ?? string.Empty)
            ? filter.SortBy
            : "CreatedAt";

        var sortDirection = filter.SortDirection == SortDirection.Asc ? "ASC" : "DESC";

        var dataSql = $"""
            SELECT Id, Title, Description, Status, DueDate, UserId, CreatedAt, UpdatedAt
            FROM Tasks
            WHERE UserId = @UserId
            """;

        if (whereConditions.Count > 0)
        {
            dataSql += " AND " + string.Join(" AND ", whereConditions);
        }

        dataSql += $"""
            
            ORDER BY {sortColumn} {sortDirection}
            LIMIT @PageSize OFFSET @Offset
            """;

        await using var dataCommand = connection.CreateCommand();
        dataCommand.CommandText = dataSql;

        foreach (var param in parameters) dataCommand.Parameters.Add(new SqliteParameter(param.ParameterName, param.Value));
        dataCommand.Parameters.Add(new("@PageSize", filter.PageSize));
        dataCommand.Parameters.Add(new("@Offset", (filter.Page - 1) * filter.PageSize));

        var items = new List<TaskItem>();
        await using var reader = await dataCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapTask(reader));
        }

        return new PagedResult<TaskItem>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
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
