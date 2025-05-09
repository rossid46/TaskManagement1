﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagement.DataAccess.Interfaces;
using TaskManagement.Models;
using TaskManagement.Models.ViewModels;
using TaskManagement.Utility;

namespace TaskManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin, AuthenticationSchemes = "Identity.Application")]
    public class TaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public TaskController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {

            var taskList = _unitOfWork.TaskItem.GetAll(includeProperties: "ApplicationUser");
            foreach (var task in taskList)
            {
                task.ApplicationUserId = _unitOfWork.ApplicationUser.Get(u => u.Id == task.ApplicationUserId).Email;
            }
            return View(taskList);
        }
        public IActionResult Upsert(int? id)
        {
            TaskItemVM taskItemVM = new()
            {
                UserList = _unitOfWork.ApplicationUser.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                TaskItem = new TaskItem() { }
            };
            if(id==null || id==0)
            {
                //create
                return View(taskItemVM);
            }
            else
            {
                //update
                taskItemVM.TaskItem = _unitOfWork.TaskItem.Get(u=>u.Id == id);
                return View(taskItemVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(TaskItemVM taskItemVM)
        {
            if (ModelState.IsValid)
            {
                if (taskItemVM.TaskItem.Id == 0)
                {
                    _unitOfWork.TaskItem.Add(taskItemVM.TaskItem);
                }
                else
                {
                    _unitOfWork.TaskItem.Update(taskItemVM.TaskItem);
                }
                _unitOfWork.Save();
                TempData["success"] = "Task created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                //taskItemVM.UserList = _unitOfWork.TaskItem.GetAll().Select(u => new SelectListItem
                //{
                //    Text = u.Title,
                //    Value = u.Id.ToString()
                //});
                return View(taskItemVM);
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            var task = _unitOfWork.TaskItem.Get(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var task = _unitOfWork.TaskItem.Get(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            _unitOfWork.TaskItem.Remove(task);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var taskList = _unitOfWork.TaskItem.GetAll(includeProperties: "ApplicationUser");
            foreach (var task in taskList)
            {
                task.ApplicationUserId = _unitOfWork.ApplicationUser.Get(u => u.Id == task.ApplicationUserId).Name;
            }
            return Json(new {data = taskList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var taskToBeDeleted = _unitOfWork.TaskItem.Get(u => u.Id == id);
            if (taskToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            
            _unitOfWork.TaskItem.Remove(taskToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
