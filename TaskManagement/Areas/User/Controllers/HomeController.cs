using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.DataAccess.Repository.IRepository;
using TaskManagement.Models;

namespace TaskManagement.Web.Areas.User.Controllers;
[Area("User")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        IEnumerable<TaskItem> tasks = _unitOfWork.TaskItem.GetAll(filter: null, includeProperties: "Comments");
        return View(tasks);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
