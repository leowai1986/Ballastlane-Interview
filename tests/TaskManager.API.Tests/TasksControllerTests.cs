using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.API.Controllers;
using TaskManager.Domain.Enums;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<IValidator<CreateTaskRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateTaskRequest>> _updateValidatorMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _createValidatorMock = new Mock<IValidator<CreateTaskRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTaskRequest>>();
        _controller = new TasksController(
            _taskServiceMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);

        var userId = Guid.NewGuid().ToString();
        var claims = new[] { new Claim("userId", userId) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetById_WithExistingTask_Returns200()
    {
        var taskId = Guid.NewGuid();
        var taskDto = new TaskDto { Id = taskId, Title = "Test" };
        _taskServiceMock.Setup(s => s.GetTaskAsync(It.IsAny<Guid>(), taskId, default))
            .ReturnsAsync(taskDto);

        var result = await _controller.GetById(taskId, default);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(taskDto, okResult.Value);
    }

    [Fact]
    public async Task GetById_WithNonExistingTask_Returns404()
    {
        _taskServiceMock.Setup(s => s.GetTaskAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((TaskDto?)null);

        var result = await _controller.GetById(Guid.NewGuid(), default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithValidRequest_Returns201()
    {
        var request = new CreateTaskRequest { Title = "New Task" };
        var created = new TaskDto { Id = Guid.NewGuid(), Title = "New Task" };
        _taskServiceMock.Setup(s => s.CreateTaskAsync(It.IsAny<Guid>(), request, default))
            .ReturnsAsync(created);

        var result = await _controller.Create(request, default);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(created, createdResult.Value);
    }

    [Fact]
    public async Task Update_WithNonExistingTask_Returns404()
    {
        _taskServiceMock.Setup(s => s.GetTaskAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((TaskDto?)null);

        var result = await _controller.Update(Guid.NewGuid(), new UpdateTaskRequest(), default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_WithValidRequest_Returns204()
    {
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(s => s.GetTaskAsync(It.IsAny<Guid>(), taskId, default))
            .ReturnsAsync(new TaskDto { Id = taskId });
        _taskServiceMock.Setup(s => s.UpdateTaskAsync(taskId, It.IsAny<UpdateTaskRequest>(), default))
            .ReturnsAsync(true);

        var result = await _controller.Update(taskId, new UpdateTaskRequest { Title = "Updated", Status = TaskManager.Domain.Enums.TaskStatus.InProgress }, default);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistingTask_Returns404()
    {
        _taskServiceMock.Setup(s => s.GetTaskAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((TaskDto?)null);

        var result = await _controller.Delete(Guid.NewGuid(), default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WithExistingTask_Returns204()
    {
        var taskId = Guid.NewGuid();
        _taskServiceMock.Setup(s => s.GetTaskAsync(It.IsAny<Guid>(), taskId, default))
            .ReturnsAsync(new TaskDto { Id = taskId });
        _taskServiceMock.Setup(s => s.DeleteTaskAsync(taskId, default))
            .ReturnsAsync(true);

        var result = await _controller.Delete(taskId, default);

        Assert.IsType<NoContentResult>(result);
    }
}