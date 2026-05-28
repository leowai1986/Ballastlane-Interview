using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Repositories;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskItem>> GetByUserIdPaginatedAsync(Guid userId, TaskFilterRequest filter, CancellationToken cancellationToken = default);
    Task<TaskItem> CreateAsync(TaskItem taskItem, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
