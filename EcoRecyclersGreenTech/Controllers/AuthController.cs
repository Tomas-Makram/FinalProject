using Microsoft.AspNetCore.Mvc;
using EcoRecyclersGreenTech.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using EcoRecyclersGreenTech.Data.Users;

namespace EcoRecyclersGreenTech.Controllers
{
    public class AuthController(DBContext db) : Controller
    {
        private readonly DBContext _db = db;

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginDataModel login)
        {
            var user = _db.Users
                .FirstOrDefault(u =>
                    (u.Email == login.username ||
                     u.phoneNumber == login.username ||
                     u.FullName == login.username) &&
                    u.Password == login.password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserEmail", user.Email!);
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("UserName", user.FullName ?? "");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid login credentials";
            return View();
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupDataModel signup)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Check email duplication
                if (_db.Users.Any(u => u.Email == signup.user.Email))
                {
                    ViewBag.Error = "This Account is Used Before";
                    return View();
                }

                // Default user values
                signup.user.JoinDate = DateTime.Now;
                signup.user.Verified = false;
                signup.user.Blocked = false;

                // Save Additional Type Data First
                int realTypeId = 0;

                switch (signup.type!.TypeName)
                {
                    case "Individual":
                        _db.Individuals.Add(signup.individual!);
                        await _db.SaveChangesAsync();
                        realTypeId = signup.individual!.IndividualID;
                        //signup.type!.TypeName = "Individual";
                        break;

                    case "Factory":
                        _db.Factories.Add(signup.factory!);
                        await _db.SaveChangesAsync();
                        realTypeId = signup.factory!.FactoryID;
                        //signup.type!.TypeName = "Factory";
                        break;

                    case "Craftsman":
                        _db.Craftsmen.Add(signup.craftsman!);
                        await _db.SaveChangesAsync();
                        realTypeId = signup.craftsman!.CraftsmanID;
                        //signup.type!.TypeName = "Craftsman";
                        break;
                }

                // Now save UserType with correct RealTypeID
                signup.type!.RealTypeID = realTypeId;
                _db.UserTypes.Add(signup.type);
                await _db.SaveChangesAsync();

                // Connect User with Type
                signup.user.UserTypeID = signup.type.TypeID;
                _db.Users.Add(signup.user);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                // Auto login
                HttpContext.Session.SetString("UserEmail", signup.user.Email!);
                HttpContext.Session.SetInt32("UserID", signup.user.UserID);
                HttpContext.Session.SetString("UserName", signup.user.FullName ?? "");
                HttpContext.Session.SetInt32("UserTypeID", signup.user.UserTypeID);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
