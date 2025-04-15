namespace TaskManagement.DataAccess.Interfaces
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using TaskManagement.Models;
    using TaskManagement.Models.ViewModels;
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        TaskItem GetAsync(int id);
        Task<(bool IsValid, IEnumerable<string> Errors, TaskItem? Task)> UpsertTaskAsync(int id, TaskItemVM taskItemVM, ClaimsPrincipal user);
        bool DeleteTask(int id, ClaimsPrincipal user);
    }
}
