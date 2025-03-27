using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.DataAccess.Repository.IRepository;
using TaskManagement.Models;
using TaskManagement.Utility;

namespace TaskManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class TaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public TaskController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var tasks = _unitOfWork.TaskItem.GetAll(null,includeProperties:"Comments");
            return View(tasks);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TaskItem task)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.TaskItem.Add(task);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        public IActionResult Edit(int id)
        {
            var task = _unitOfWork.TaskItem.Get(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult Edit(int id, TaskItem task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.TaskItem.Update(task);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(task);
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

        public IActionResult Assign(int taskId, int userId)
        {
            var task = _unitOfWork.TaskItem.Get(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound();
            }

            task.AssignedToUserId = userId;
            _unitOfWork.TaskItem.Update(task);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }


    }
}
