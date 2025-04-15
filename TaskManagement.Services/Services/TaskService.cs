using TaskManagement.Models;
using TaskManagement.Models.ViewModels;
using TaskManagement.DataAccess.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<TaskItemVM> _validator;
    private readonly ILogger<TaskService> _logger;

    public TaskService(IUnitOfWork unitOfWork, IValidator<TaskItemVM> validator, ILogger<TaskService> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
    {
        return _unitOfWork.TaskItem.GetAll(includeProperties: "ApplicationUser");
    }

    public TaskItem GetAsync(int id)
    {
        TaskItem taskItem = _unitOfWork.TaskItem.Get(x=>x.Id == id, includeProperties: "ApplicationUser");
        if (taskItem == null)
        {
            throw new KeyNotFoundException($"TaskItem with ID {id} not found.");
        }
        return taskItem;
    }

    public async Task<(bool IsValid, IEnumerable<string> Errors, TaskItem? Task)> UpsertTaskAsync(int id, TaskItemVM taskItemVM, ClaimsPrincipal user)
    {
        var resultValidation = await _validator.ValidateAsync(taskItemVM);
        if (!resultValidation.IsValid)
        {
            var errors = resultValidation.Errors.Select(e => e.ErrorMessage);
            return (false, errors, null);
        }

        if (taskItemVM.TaskItem.Id == 0)
        {
            _unitOfWork.TaskItem.Add(taskItemVM.TaskItem);
            _unitOfWork.Save();
            _logger.LogInformation("Task created successfully.");
            return (true, Enumerable.Empty<string>(), taskItemVM.TaskItem);
        }
        else
        {
            var oldTaskItem = _unitOfWork.TaskItem.Get(x => x.Id == taskItemVM.TaskItem.Id);
            if (oldTaskItem == null)
            {
                return (false, new[] { "Task not found." }, null);
            }

            if (oldTaskItem.Status != taskItemVM.TaskItem.Status)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                History history = new History
                {
                    FromStatus = oldTaskItem.Status,
                    ToStatus = taskItemVM.TaskItem.Status,
                    ChangeDate = DateTime.Now,
                    ApplicationUserId = userId,
                    TaskItemId = taskItemVM.TaskItem.Id
                };
                _unitOfWork.History.Add(history);
                _unitOfWork.Save();
            }

            oldTaskItem.Description = taskItemVM.TaskItem.Description;
            oldTaskItem.DueDate = taskItemVM.TaskItem.DueDate;
            _unitOfWork.TaskItem.Update(oldTaskItem);
            _unitOfWork.Save();
            _logger.LogInformation("TaskItem updated successfully.");
            return (true, Enumerable.Empty<string>(), oldTaskItem);
        }
    }

    public bool DeleteTask(int id, ClaimsPrincipal user)
    {
        var taskItem = _unitOfWork.TaskItem.Get(x => x.Id == id);
        if (taskItem == null)
        {
            return false;
        }
        _unitOfWork.TaskItem.Remove(taskItem);
        _unitOfWork.Save();
        _logger.LogInformation("TaskItem deleted successfully.");
        return true;

    }
}
