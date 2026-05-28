using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskItem>> GetUserTasksAsync(Guid userId, TaskFilterRequest filter, CancellationToken cancellationToken = default);
    Task<TaskDto?> GetTaskAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateTaskAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateTaskAsync(Guid id, Guid userId, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
