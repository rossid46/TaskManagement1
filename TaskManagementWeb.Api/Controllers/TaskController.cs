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
        private readonly IValidator<TaskItemVM> _validator;
        private readonly ILogger<TaskController> _logger;
        private readonly ITaskService _taskService;

        public TaskController(IUnitOfWork unitOfWork, IValidator<TaskItemVM> validator, ILogger<TaskController> logger, ITaskService taskService)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _logger = logger;
            _taskService = taskService;
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<TaskItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var taskItems = await _taskService.GetAllTasksAsync();
            return Ok(taskItems.ToList());
        }

        [HttpGet("Get/{id}")]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(int id)
        {
            var taskItem =  _taskService.GetAsync(id);
            
            return Ok(taskItem);
        }

        [HttpPost("Upsert/{id}")]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upsert(int id, [FromBody] TaskItemVM taskItemVM)
        {
            var result = await _taskService.UpsertTaskAsync(id, taskItemVM, User);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Task);

        }

        [HttpDelete("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var result = _taskService.DeleteTask(id, new ClaimsPrincipal());
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

    }
}
