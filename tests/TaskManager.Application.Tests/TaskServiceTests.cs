using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Services;
using TaskManager.Application.Validators.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Repositories;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Tests;

public class TaskServiceTests
{
    [Fact]
    public async Task CreateTaskAsync_WithValidRequest_ReturnsTaskDto()
    {
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest { Title = "My Task", Description = "Desc" };
        var repoMock = new Mock<ITaskRepository>();
        var validatorMock = CreateValidValidator();

        repoMock
            .Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem item, CancellationToken _) => item);

        var sut = new TaskService(repoMock.Object, validatorMock.Object);

        var result = await sut.CreateTaskAsync(userId, request);

        result.Title.Should().Be("My Task");
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(DomainTaskStatus.Pending);
    }

    [Fact]
    public async Task CreateTaskAsync_WithEmptyTitle_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest { Title = string.Empty, Description = "Desc" };
        var repoMock = new Mock<ITaskRepository>();
        var sut = new TaskService(repoMock.Object, new CreateTaskRequestValidator());

        var act = async () => await sut.CreateTaskAsync(userId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetTaskAsync_ReturnsNull_IfTaskBelongsToAnotherUser()
    {
        var currentUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var repoMock = new Mock<ITaskRepository>();
        var validatorMock = CreateValidValidator();

        repoMock
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskItem
            {
                Id = taskId,
                Title = "t",
                Description = "d",
                Status = DomainTaskStatus.Pending,
                UserId = ownerId
            });

        var sut = new TaskService(repoMock.Object, validatorMock.Object);

        var result = await sut.GetTaskAsync(currentUserId, taskId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserTasksAsync_ReturnsOnlyThatUsersTasks()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<ITaskRepository>();
        var validatorMock = CreateValidValidator();
        repoMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>
            {
                new() { Id = Guid.NewGuid(), Title = "Task 1", UserId = userId, Status = DomainTaskStatus.Pending },
                new() { Id = Guid.NewGuid(), Title = "Task 2", UserId = userId, Status = DomainTaskStatus.InProgress }
            });

        var sut = new TaskService(repoMock.Object, validatorMock.Object);

        var result = await sut.GetUserTasksAsync(userId);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.UserId == userId);
    }

    private static Mock<IValidator<CreateTaskRequest>> CreateValidValidator()
    {
        var validatorMock = new Mock<IValidator<CreateTaskRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateTaskRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return validatorMock;
    }
}
