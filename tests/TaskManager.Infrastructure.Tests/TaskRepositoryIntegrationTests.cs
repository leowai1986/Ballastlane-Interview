using Microsoft.Data.Sqlite;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using TaskStatusEnum = TaskManager.Domain.Enums.TaskStatus;

public class TaskRepositoryIntegrationTests : IDisposable
{
    private readonly string _dbFile;
    private readonly TaskRepository _repository;
    private readonly UserRepository _userRepository;

    public TaskRepositoryIntegrationTests()
    {
        _dbFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");

        var conn = new SqliteConnection($"DataSource={_dbFile}");
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE Tasks (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Description TEXT,
                Status TEXT NOT NULL,
                DueDate TEXT,
                UserId TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            insert into Tasks values ('a2222222-2222-2222-2222-222222222221', 'User Task', 'User Task', 'Pending', '2027-01-01', '11111111-1111-1111-1111-111111111111', '2024-01-01', '2024-01-01');
            CREATE TABLE Users (
                Id TEXT PRIMARY KEY,
                Email TEXT UNIQUE,
                PasswordHash TEXT,
                Name TEXT,
                CreatedAt TEXT,
                UpdatedAt TEXT
            );
            INSERT INTO Users VALUES ('11111111-1111-1111-1111-111111111111', 'demo@test.com', 'hash', 'Demo', '2024-01-01', '2024-01-01');";
        cmd.ExecuteNonQuery();

        conn.Close();
        conn.Dispose();

        var factory = new TestConnectionFactory($"DataSource={_dbFile}");
        _repository = new TaskRepository(factory);
        _userRepository = new UserRepository(factory);
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertTask()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = TaskStatusEnum.Pending,
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repository.CreateAsync(task);

        Assert.NotNull(result);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask()
    {
        var id = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = id,
            Title = "Find Me",
            Status = TaskStatusEnum.Pending,
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(task);

        var found = await _repository.GetByIdAsync(id);

        Assert.NotNull(found);
        Assert.Equal("Find Me", found.Title);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyUserTasks()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var tasks = await _repository.GetByUserIdPaginatedAsync(userId, new TaskFilterRequest(), CancellationToken.None);

        Assert.All(tasks.Items, t => Assert.Equal(userId, t.UserId));
    }

    [Fact]
    public async Task GetByIdAsync_UserExists_ReturnsUser()
    {
        var expectedUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            Name = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userRepository.GetByIdAsync(expectedUser.Id);

        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UserDoesNotExist_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _userRepository.GetByIdAsync(nonExistentId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyTask()
    {
        var id = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = id,
            Title = "Original",
            Status = TaskStatusEnum.Pending,
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(task);

        task.Title = "Updated";
        await _repository.UpdateAsync(task);

        var updated = await _repository.GetByIdAsync(id);
        Assert.Equal("Updated", updated!.Title);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        var id = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = id,
            Title = "To Delete",
            Status = TaskStatusEnum.Pending,
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(task);

        await _repository.DeleteAsync(id);

        var deleted = await _repository.GetByIdAsync(id);
        Assert.Null(deleted);
    }

    public void Dispose()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();

        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (File.Exists(_dbFile))
                {
                    File.Delete(_dbFile);
                }
                break;
            }
            catch
            {
                Thread.Sleep(100);
            }
        }
    }
}

public class TestConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public TestConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        return conn;
    }
}