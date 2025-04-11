
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.ProjectModel;
using System.Linq.Expressions;
using System.Security.Claims;
using TaskManagement.DataAccess.Interfaces;
using TaskManagement.Models;
using TaskManagement.Models.ViewModels;
using TaskManagementWeb.Api.Controllers;

public class TaskControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<TaskController>> _mockLogger;
    private readonly Mock<IValidator<TaskItemVM>> _mockValidator;
    private readonly TaskController _controller;

    public TaskControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<TaskController>>();
        _mockValidator = new Mock<IValidator<TaskItemVM>>();

        _controller = new TaskController(_mockUnitOfWork.Object, _mockValidator.Object, _mockLogger.Object);

        // Simula un utente autenticato
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public void GetAll_ReturnsOkResult_WithTaskItems()
    {
        // Arrange
        var taskItems = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1" },
            new TaskItem { Id = 2, Title = "Task 2" }
        };
        _mockUnitOfWork.Setup(u => u.TaskItem.GetAll(It.IsAny<string>())).Returns(taskItems);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTaskItems = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(okResult.Value);
        Assert.Equal(2, returnedTaskItems.Count());
    }

    [Fact]
    public void Get_ReturnsNotFound_WhenTaskItemDoesNotExist()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>()))
            .Returns((TaskItem)null);

        // Act
        var result = _controller.Get(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Get_ReturnsOkResult_WithTaskItem()
    {
        // Arrange
        var taskItem = new TaskItem { Id = 1, Title = "Task 1" };
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Func<TaskItem, bool>>(), It.IsAny<string>()))
            .Returns(taskItem);

        // Act
        var result = _controller.Get(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTaskItem = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal(1, returnedTaskItem.Id);
    }

    [Fact]
    public async Task Upsert_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var taskItemVM = new TaskItemVM();
        _mockValidator.Setup(v => v.ValidateAsync(taskItemVM, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Title", "The Title field is required.")
            }));

        // Act
        var result = await _controller.Upsert(0, taskItemVM);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
        Assert.Contains("The Title field is required.", errors);
    }

    [Fact]
    public async Task Upsert_CreatesTaskItem_WhenIdIsZero()
    {
        // Arrange
        var taskItemVM = new TaskItemVM
        {
            TaskItem = new TaskItem { Id = 0, Title = "New Task" }
        };
        _mockValidator.Setup(v => v.ValidateAsync(taskItemVM, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Act
        var result = await _controller.Upsert(0, taskItemVM);

        // Assert
        _mockUnitOfWork.Verify(u => u.TaskItem.Add(It.IsAny<TaskItem>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var createdTaskItem = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("New Task", createdTaskItem.Title);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenTaskItemDoesNotExist()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>()))
            .Returns((TaskItem)null);

        // Act
        var result = _controller.Delete(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_RemovesTaskItem_WhenTaskItemExists()
    {
        // Arrange
        var taskItem = new TaskItem { Id = 1, Title = "Task to Delete" };
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>()))
            .Returns(taskItem);

        // Act
        var result = _controller.Delete(1);

        // Assert
        _mockUnitOfWork.Verify(u => u.TaskItem.Remove(taskItem), Times.Once);
        _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Task deleted successfully", ((dynamic)okResult.Value).message);
    }
}
