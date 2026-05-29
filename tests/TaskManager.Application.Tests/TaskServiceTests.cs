using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
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
        var updateValidatorMock = UpdateValidValidator();

        repoMock
            .Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem item, CancellationToken _) => item);

        var sut = new TaskService(repoMock.Object, validatorMock.Object, updateValidatorMock.Object);

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
        var sut = new TaskService(repoMock.Object, new CreateTaskRequestValidator(), new UpdateTaskRequestValidator());

        var act = async () => await sut.CreateTaskAsync(userId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateTaskAsync_WithEmptyTitle_ThrowsValidationException()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTaskRequest { Title = string.Empty, Description = "Desc" };
        var repoMock = new Mock<ITaskRepository>();
        var sut = new TaskService(repoMock.Object, new CreateTaskRequestValidator(), new UpdateTaskRequestValidator());

        var act = async () => await sut.UpdateTaskAsync(taskId, userId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetTaskAsync_ReturnsNull_IfTaskBelongsToAnotherUser()
    {
        var currentUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var repoMock = new Mock<ITaskRepository>();
        var createValidatorMock = CreateValidValidator();
        var updateValidatorMock = UpdateValidValidator();

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

        var sut = new TaskService(repoMock.Object, createValidatorMock.Object, updateValidatorMock.Object);

        var result = await sut.GetTaskAsync(currentUserId, taskId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserTasksAsync_ReturnsOnlyThatUsersTasks()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<ITaskRepository>();
        var validatorMock = CreateValidValidator();
        var updateValidatorMock = UpdateValidValidator();
        repoMock
            .Setup(r => r.GetByUserIdPaginatedAsync(userId, It.IsAny<TaskFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<TaskItem>
            {
                Items =
                [
                    new() { Id = Guid.NewGuid(), Title = "Task 1", UserId = userId, Status = DomainTaskStatus.Pending },
                    new() { Id = Guid.NewGuid(), Title = "Task 2", UserId = userId, Status = DomainTaskStatus.InProgress }
                ],
                Page = 1,
                PageSize = 20,
                TotalCount = 2
            });

        var sut = new TaskService(repoMock.Object, validatorMock.Object, updateValidatorMock.Object);

        var result = await sut.GetUserTasksAsync(userId, new TaskFilterRequest(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.UserId == userId);
    }

    [Fact]
    public async Task GetTaskAsync_ReturnsTask()
    {
        var currentUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var repoMock = new Mock<ITaskRepository>();
        var validatorMock = CreateValidValidator();
        var updateValidatorMock = UpdateValidValidator();

        repoMock
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskItem
            {
                Id = taskId,
                Title = "t",
                Description = "d",
                Status = DomainTaskStatus.Pending,
                UserId = currentUserId
            });

        var sut = new TaskService(repoMock.Object, validatorMock.Object, updateValidatorMock.Object);

        var result = await sut.GetTaskAsync(currentUserId, taskId);

        Assert.NotNull(result);
        Assert.Equal(taskId, result.Id);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithExistingTask_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repoMock = new Mock<ITaskRepository>();
        var validatorMock = CreateValidValidator();
        var updateValidatorMock = UpdateValidValidator();
        var existing = new TaskItem { Id = id, Title = "Old", UserId = userId, Status = DomainTaskStatus.Pending };
        repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(existing);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<TaskItem>(), default)).ReturnsAsync(true);
        var sut = new TaskService(repoMock.Object, validatorMock.Object, updateValidatorMock.Object);
        var result = await sut.UpdateTaskAsync(id, userId, new UpdateTaskRequest { Title = "New", Status = DomainTaskStatus.InProgress }, default);

        Assert.True(result);
        repoMock.Verify(r => r.UpdateAsync(It.Is<TaskItem>(t => t.Title == "New"), default), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_WithNonExistingTask_ThrowsNotFound()
    {
        var repoMock = new Mock<ITaskRepository>();
        var validatorMock = CreateValidValidator();
        var updateValidatorMock = UpdateValidValidator();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((TaskItem?)null);
        var sut = new TaskService(repoMock.Object, validatorMock.Object, updateValidatorMock.Object);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateTaskAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateTaskRequest { Title = "New", Status = DomainTaskStatus.Pending }, default));
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

    private static Mock<IValidator<UpdateTaskRequest>> UpdateValidValidator()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return validatorMock;
    }
}
