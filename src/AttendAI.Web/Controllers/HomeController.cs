using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AttendAI.Web.Models;

namespace AttendAI.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult StatusCodePage(int code = 404)
    {
        Response.StatusCode = code;
        return code == StatusCodes.Status404NotFound
            ? View("NotFound")
            : View("StatusCode", code);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
