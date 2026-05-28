using Microsoft.AspNetCore.Http;
using TaskManager.API.Middleware;
using TaskManager.Application.Exceptions;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task HandleExceptionAsync_WithNotFoundException_Returns404()
    {
        var context = new DefaultHttpContext();
        var exception = new NotFoundException("Not found");

        await GlobalExceptionHandlerMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal(404, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleExceptionAsync_WithConflictException_Returns409()
    {
        var context = new DefaultHttpContext();
        var exception = new ConflictException("Conflict");

        await GlobalExceptionHandlerMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal(409, context.Response.StatusCode);
    }

    [Fact]
    public async Task HandleExceptionAsync_WithUnauthorizedException_Returns401()
    {
        var context = new DefaultHttpContext();
        var exception = new UnauthorizedException("Unauthorized");

        await GlobalExceptionHandlerMiddleware.HandleExceptionAsync(context, exception);

        Assert.Equal(401, context.Response.StatusCode);
    }
}