namespace TaskManager.Domain.Tests;

using TaskStatusEnum = Enums.TaskStatus;

public class TaskStatusTests
{
    [Theory]
    [InlineData(1, TaskStatusEnum.Pending)]
    [InlineData(2, TaskStatusEnum.InProgress)]
    [InlineData(3, TaskStatusEnum.Completed)]
    public void Should_Parse_From_Int(int value, TaskStatusEnum expected)
    {
        var result = (TaskStatusEnum)value;
        Assert.Equal(expected, result);
    }
}