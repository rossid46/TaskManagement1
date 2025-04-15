
using FluentValidation;
using FluentValidation.Results;
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
    private readonly Mock<ITaskService> _mockTaskService;
    private readonly TaskController _controller;

    public TaskControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<TaskController>>();
        _mockValidator = new Mock<IValidator<TaskItemVM>>();
        _mockTaskService = new Mock<ITaskService>();


        _controller = new TaskController(_mockUnitOfWork.Object, _mockValidator.Object, _mockLogger.Object, _mockTaskService.Object);

        _mockUnitOfWork.Setup(db => db.TaskItem.Add(It.IsAny<TaskItem>()));

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemVM>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());


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

        // Act
        _mockTaskService.Setup(s => s.GetAllTasksAsync())
            .ReturnsAsync(taskItems);
        var result = _controller.GetAllAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTaskItems = Assert.IsType<List<TaskItem>>(okResult.Value);
        Assert.Equal(2, returnedTaskItems.Count);
    }

    [Fact]
    public void Get_ReturnsOkResult_WithTaskItem()
    {
        var taskItem = new TaskItem { Id = 1, Title = "Task 1" };
        _mockTaskService.Setup(s => s.GetAsync(1))
            .Returns(taskItem);
        // Act
        var result = _controller.Get(1);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTaskItem = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal(1, returnedTaskItem.Id);
    }

    [Fact]
    public async Task Upsert_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemVM>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult
                {
                    Errors = new List<ValidationFailure>() {
                    new ValidationFailure("Title", "Required")
                }
                });
        // Arrange
        var taskItemVM = new TaskItemVM
        {
            TaskItem = new TaskItem 
            { 
                Id = 1,
                Title = "" // Invalid title
            }
        };
        // Act
        var result = await _controller.Upsert(1, taskItemVM);
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upsert_CreatesTaskItem_WhenIdIsZero()
    {
        // Arrange
        var taskItemVM = new TaskItemVM
        {
            TaskItem = new TaskItem
            {
                Id = 0,
                Title = "New Task"
            }
        };
        _mockTaskService.Setup(s => s.UpsertTaskAsync(0, taskItemVM, It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((true, null, taskItemVM.TaskItem));
        // Act
        var result = await _controller.Upsert(0, taskItemVM);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTaskItem = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal("New Task", returnedTaskItem.Title);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenTaskItemDoesNotExist()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
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
        _mockUnitOfWork.Setup(u => u.TaskItem.Get(It.IsAny<Expression<Func<TaskItem, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(taskItem);
        _mockTaskService.Setup(s => s.DeleteTask(1, It.IsAny<ClaimsPrincipal>())).Returns(true);

        // Act
        var result = _controller.Delete(1);
        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

    }
}
