using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.DataAccess.Repository.IRepository;
using TaskManagement.Models;
using TaskManagement.Models.ViewModels;

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

        if (User.Identity.IsAuthenticated)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<TaskItem> taskList = _unitOfWork.TaskItem.GetAll(includeProperties: "Comments,ApplicationUser").ToList();
            return View(taskList);
        }
        return Redirect("identity/account/login");
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
