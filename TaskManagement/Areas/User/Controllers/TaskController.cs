using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using TaskManagement.DataAccess.Interfaces;
using TaskManagement.Models;
using TaskManagement.Models.ViewModels;
using TaskManagement.Utility;

namespace TaskManagement.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles= SD.Role_User, AuthenticationSchemes = "Identity.Application")]
    public class TaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public TaskController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult UpdateStatus(int? id)
        {
            if (id == 0 || id == null)
                return NotFound();

            TaskItem taskItem = _unitOfWork.TaskItem.Get(u => u.Id == id);

            TaskItemVM taskItemVM = new()
            {
                UserList = _unitOfWork.ApplicationUser.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                TaskItem = taskItem
            };
            if (taskItemVM == null)
                return NotFound();

            return View(taskItemVM);
        }
        [HttpPost]
        public IActionResult UpdateStatus(TaskItemVM taskItemVM)
        {
            if (ModelState.IsValid)
            {
                var oldTaskItem = _unitOfWork.TaskItem.Get(t => t.Id == taskItemVM.TaskItem.Id);
                if (oldTaskItem.Status != taskItemVM.TaskItem.Status)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                    var history = new History()
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
                ;
                _unitOfWork.TaskItem.Update(taskItemVM.TaskItem);
                _unitOfWork.Save();
                TempData["success"] = "Task updated successfully";
                return RedirectToAction("Index", "Home");
            }
            return View(taskItemVM);
        }

    }
}
