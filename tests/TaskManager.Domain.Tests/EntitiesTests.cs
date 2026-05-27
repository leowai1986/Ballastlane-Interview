using FluentAssertions;
using TaskManager.Domain.Entities;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Tests;

public class EntitiesTests
{
    [Fact]
    public void TaskItem_DefaultStatus_IsPending()
    {
        var task = new TaskItem();

        task.Status.Should().Be(DomainTaskStatus.Pending);
    }

    [Fact]
    public void User_Creation_WithValidData_SetsProperties()
    {
        var email = "demo@ballastlane.com";
        var passwordHash = "hash";
        var name = "Demo User";

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Name = name
        };

        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.Name.Should().Be(name);
        user.Id.Should().NotBeEmpty();
    }
}
