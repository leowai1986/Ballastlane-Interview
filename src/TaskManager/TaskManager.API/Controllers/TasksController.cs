using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;

    public TasksController(
        ITaskService taskService,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator)
    {
        _taskService = taskService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetUserTasksAsync(userId, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var task = await _taskService.GetTaskAsync(userId, id, cancellationToken);
        if (task is null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var userId = GetUserId();
        var created = await _taskService.CreateTaskAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var userId = GetUserId();
        var existing = await _taskService.GetTaskAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var updated = await _taskService.UpdateTaskAsync(id, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var existing = await _taskService.GetTaskAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var deleted = await _taskService.DeleteTaskAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdValue = User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found.");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID claim.");
        }

        return userId;
    }
}
