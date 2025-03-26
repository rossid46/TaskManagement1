using Microsoft.AspNetCore.Mvc;
using TaskManagement.DataAccess.Repository.IRepository;
using TaskManagement.Models;

namespace TaskManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public TaskController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var tasks = await _unitOfWork.UserTaskRepository.GetAll();
            return View(tasks);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserTask task)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.UserTaskRepository.Add(task);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _unitOfWork.UserTaskRepository.GetById(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserTask task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.UserTaskRepository.Update(task);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _unitOfWork.UserTaskRepository.GetById(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _unitOfWork.UserTaskRepository.Delete(id);
            await _unitOfWork.CompleteAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Assign(int taskId, int userId)
        {
            var task = await _unitOfWork.UserTaskRepository.GetById(taskId);
            if (task == null)
            {
                return NotFound();
            }

            task.AssignedToUserId = userId;
            _unitOfWork.UserTaskRepository.Update(task);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
