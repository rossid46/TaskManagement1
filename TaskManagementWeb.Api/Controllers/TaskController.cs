using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.DataAccess.Interfaces;
using TaskManagement.Models;
using TaskManagement.Models.ViewModels;
using TaskManagement.Utility;

namespace TaskManagementWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator _validator;
        private readonly ILogger<TaskController> _logger;

        public TaskController(IUnitOfWork unitOfWork, IValidator validator, ILogger<TaskController> logger)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _logger = logger;
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<TaskItem>), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            var taskItems = _unitOfWork.TaskItem.GetAll(includeProperties: "ApplicationUser");
            return Ok(taskItems);
        }

        [HttpGet("Get/{id}")]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(int id)
        {
            var taskItem = _unitOfWork.TaskItem.Get(x => x.Id == id, includeProperties: "ApplicationUser");
            if (taskItem == null)
            {
                return NotFound();
            }
            return Ok(taskItem);
        }

        [HttpPost("Upsert/{id}")]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upsert(int id, [FromBody] TaskItemVM taskItemVM)
        {
            var resultValidation = await _validator.ValidateAsync((IValidationContext)taskItemVM);
            if (!resultValidation.IsValid)
            {
                resultValidation.AddToModelState(ModelState);
                var errors = new List<string>();
                foreach (var error in resultValidation.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
                return BadRequest(errors);
            }
            if (taskItemVM.TaskItem.Id == 0)
            {
                _unitOfWork.TaskItem.Add(taskItemVM.TaskItem);
                _unitOfWork.Save();
                _logger.LogInformation("TaskItem created successfully.");
                return Ok(taskItemVM.TaskItem);
            }
            else
            {
                var oldTaskItem = _unitOfWork.TaskItem.Get(x => x.Id == taskItemVM.TaskItem.Id);
                if (oldTaskItem.Status != taskItemVM.TaskItem.Status)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
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
                _unitOfWork.TaskItem.Update(taskItemVM.TaskItem);
                _unitOfWork.Save();
                TempData["success"] = "Task updated successfully";

                if (oldTaskItem.ApplicationUserId != null)
                {
                    oldTaskItem.ApplicationUserId = taskItemVM.TaskItem.ApplicationUserId;
                }
                oldTaskItem.Description = taskItemVM.TaskItem.Description;
                oldTaskItem.DueDate = taskItemVM.TaskItem.DueDate;

                _unitOfWork.TaskItem.Update(oldTaskItem);
                _unitOfWork.Save();
                _logger.LogInformation("TaskItem updated successfully");
                return Ok(oldTaskItem);
            }
        }

        [HttpDelete("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (User.IsInRole(SD.Role_User))
            {
                return Unauthorized();
            }
            if (_unitOfWork.TaskItem.Get(x => x.Id == id) == null)
            {
                return NotFound();
            }
            var taskItem = _unitOfWork.TaskItem.Get(x => x.Id == id);
            _unitOfWork.TaskItem.Remove(taskItem);
            _unitOfWork.Save();
            _logger.LogInformation("Task deleted successfully");
            return Ok(new { message = "Task deleted successfully" });
        }

    }
}
