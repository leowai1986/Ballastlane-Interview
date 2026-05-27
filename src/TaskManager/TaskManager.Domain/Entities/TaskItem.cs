using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Pending;
    public DateTime? DueDate { get; set; }
    public Guid UserId { get; set; }
}
