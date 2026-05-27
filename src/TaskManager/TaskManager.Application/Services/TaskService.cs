using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Repositories;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IReadOnlyCollection<TaskDto>> GetTasksByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetByUserIdAsync(userId, cancellationToken);

        return tasks.Select(MapToDto).ToList();
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var taskItem = await _taskRepository.GetByIdAsync(id, cancellationToken);
        return taskItem is null ? null : MapToDto(taskItem);
    }

    public async Task<TaskDto> CreateTaskAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var taskItem = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            DueDate = request.DueDate,
            UserId = userId
        };

        var created = await _taskRepository.CreateAsync(taskItem, cancellationToken);
        return MapToDto(created);
    }

    public async Task<bool> UpdateTaskAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new NotFoundException($"Task '{id}' was not found.");
        }

        existing.Title = request.Title.Trim();
        existing.Description = request.Description.Trim();
        existing.Status = request.Status;
        existing.DueDate = request.DueDate;
        existing.UpdatedAt = DateTime.UtcNow;

        return await _taskRepository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _taskRepository.DeleteAsync(id, cancellationToken);
    }

    private static TaskDto MapToDto(TaskItem taskItem)
    {
        return new TaskDto
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            Status = taskItem.Status,
            DueDate = taskItem.DueDate,
            UserId = taskItem.UserId,
            CreatedAt = taskItem.CreatedAt,
            UpdatedAt = taskItem.UpdatedAt
        };
    }
}
