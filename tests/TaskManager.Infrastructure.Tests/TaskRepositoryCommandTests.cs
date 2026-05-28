using FluentAssertions;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Infrastructure.Tests;

public class TaskRepositoryCommandTests
{
    [Fact]
    public async Task RepositoryFile_UsesParameterizedQueries()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null &&
               !Directory.Exists(Path.Combine(current.FullName, "src", "TaskManager", "TaskManager.Infrastructure")))
        {
            current = current.Parent;
        }

        current.Should().NotBeNull("repository root should be discoverable from test output path");
        var infrastructureProjectDir = Path.Combine(current!.FullName, "src", "TaskManager", "TaskManager.Infrastructure");
        var filePath = Path.Combine(infrastructureProjectDir, "Repositories", "TaskRepository.cs");
        var code = await File.ReadAllTextAsync(filePath);

        code.Should().Contain("@Id");
        code.Should().Contain("@UserId");
        code.Should().Contain("SqliteParameter");
        code.Should().NotContain("WHERE Id = '");
    }
}
