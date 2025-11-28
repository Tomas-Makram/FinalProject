using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcoRecyclersGreenTech.Models;
using Microsoft.AspNetCore.Authorization;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Controllers;

public class HomeController : Controller
{
    private readonly DBContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(DBContext db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var email = HttpContext.Session.GetString("UserEmail");

        if (email != null)
        {
            ViewBag.UserLoggedIn = true;
            ViewBag.Email = email;
            ViewBag.type = _db.Users.Where(u => u.Email == email).Select(u => u.UserType.TypeName).FirstOrDefault();
        }
        else
        {
            ViewBag.UserLoggedIn = false;
        }

        return View();
    }


    [Authorize]
    public IActionResult AuthTesting()
    {
        return Json("User authenticated ✔");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
