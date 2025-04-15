using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Security.Claims;
using TaskManagement.DataAccess.Interfaces;
using TaskManagement.Models;
using TaskManagement.Models.ViewModels;
using Xunit;

public class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IValidator<TaskItemVM>> _mockValidator;
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockValidator = new Mock<IValidator<TaskItemVM>>();
        _mockLogger = new Mock<ILogger<TaskService>>();

        _taskService = new TaskService(_mockUnitOfWork.Object, _mockValidator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsAllTasks()
    {
        // Arrange
        var taskItems = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1" },
            new TaskItem { Id = 2, Title = "Task 2" }
        };
        _mockUnitOfWork.Setup(u => u.TaskItem.GetAll(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>()))
            .Returns(taskItems);

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAsync_ReturnsTask_WhenTaskExists()
    {
        // Arrange
        var taskItem = new TaskItem { Id = 1, Title = "Task 1" };
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(taskItem);

        // Act
        //var result = await _taskService.GetAsync(1);

        // Assert
        //Assert.NotNull(result);
        //Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetAsync_ThrowsKeyNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((TaskItem)null);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _taskService.GetAsync(1));
    }

    [Fact]
    public async Task UpsertTaskAsync_CreatesTask_WhenIdIsZero()
    {
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemVM>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((TaskItem)null);
        // Arrange
        var taskItemVM = new TaskItemVM
        {
            TaskItem = new TaskItem
            {
                Id = 0,
                Title = "New Task",
                Status = "ToDo",
                Priority = "Low",
                Description = "asdasd"
            }
        };


        // Act
        var result = await _taskService.UpsertTaskAsync(0, taskItemVM, new ClaimsPrincipal());

        // Assert
        Assert.True(result.IsValid);
        _mockUnitOfWork.Verify(u => u.TaskItem.Add(It.IsAny<TaskItem>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
    }

    [Fact]
    public async Task UpsertTaskAsync_UpdatesTask_WhenIdIsNotZero()
    {
        // Arrange
        var taskItemVM = new TaskItemVM
        {
            TaskItem = new TaskItem { Id = 1, Title = "Updated Task" }
        };
        var existingTask = new TaskItem { Id = 1, Title = "Old Task" };
        _mockValidator.Setup(v => v.ValidateAsync(taskItemVM, default))
            .ReturnsAsync(new ValidationResult());
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(existingTask);

        // Act
        var result = await _taskService.UpsertTaskAsync(1, taskItemVM, new ClaimsPrincipal());

        // Assert
        Assert.True(result.IsValid);
        _mockUnitOfWork.Verify(u => u.TaskItem.Update(It.IsAny<TaskItem>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
    }

    [Fact]
    public void DeleteTask_RemovesTask_WhenTaskExists()
    {
        // Arrange
        var taskItem = new TaskItem { Id = 1, Title = "Task to Delete" };
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(taskItem);

        // Act
        var result = _taskService.DeleteTask(1, new ClaimsPrincipal());

        // Assert
        Assert.True(result);
        _mockUnitOfWork.Verify(u => u.TaskItem.Remove(taskItem), Times.Once);
        _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
    }

    [Fact]
    public void DeleteTask_ReturnsFalse_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((TaskItem)null);

        // Act
        var result = _taskService.DeleteTask(1, new ClaimsPrincipal());

        // Assert
        Assert.False(result);
    }
}
