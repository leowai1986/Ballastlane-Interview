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

public enum SortDirection
{
    Asc,
    Desc
}

public class TaskFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;

    // Filtering
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
}

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}