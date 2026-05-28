using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Repositories;
using FluentValidation;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IValidator<CreateTaskRequest> _createTaskValidator;

    public TaskService(ITaskRepository taskRepository, IValidator<CreateTaskRequest> createTaskValidator)
    {
        _taskRepository = taskRepository;
        _createTaskValidator = createTaskValidator;
    }

    public async Task<PagedResult<TaskItem>> GetUserTasksAsync(Guid userId, TaskFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetByUserIdPaginatedAsync(userId, filter, cancellationToken);

        return tasks;
    }

    public async Task<TaskDto?> GetTaskAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var taskItem = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (taskItem is null || taskItem.UserId != userId)
        {
            return null;
        }

        return MapToDto(taskItem);
    }

    public async Task<TaskDto> CreateTaskAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await _createTaskValidator.ValidateAndThrowAsync(request, cancellationToken);

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

    public async Task<bool> UpdateTaskAsync(Guid id, Guid userId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _taskRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException($"Task '{id}' was not found.");
        if (existing.UserId != userId)
        {
            throw new UnauthorizedAccessException($"Task '{id}' not belong to user.");
        }

        existing.Title = request.Title.Trim();
        existing.Description = request.Description.Trim();
        existing.Status = request.Status;
        existing.DueDate = request.DueDate;
        existing.UpdatedAt = DateTime.UtcNow;

        return await _taskRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task<bool> DeleteTaskAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var userTask = await _taskRepository.GetByUserIdAsync(userId, cancellationToken);
        if (userTask.Any(t => t.Id == id) == false)
        {
            throw new UnauthorizedAccessException($"Task '{id}' not belong to user.");
        }
        return await _taskRepository.DeleteAsync(id, cancellationToken);
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
