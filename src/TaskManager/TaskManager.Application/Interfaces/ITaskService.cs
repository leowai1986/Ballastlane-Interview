using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskDto>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto?> GetTaskAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateTaskAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
}
