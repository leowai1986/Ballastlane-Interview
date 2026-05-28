using TaskManager.Application.Exceptions;

public class ExceptionsTests
{
    [Fact]
    public void NotFoundException_ShouldHaveMessage()
    {
        var ex = new NotFoundException("User not found");
        Assert.Equal("User not found", ex.Message);
    }

    [Fact]
    public void ConflictException_ShouldHaveMessage()
    {
        var ex = new ConflictException("Email already exists");
        Assert.Equal("Email already exists", ex.Message);
    }
}