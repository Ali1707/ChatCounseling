using ChatCounseling.Data;
using ChatCounseling.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ChatCounseling.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Context _context;

        public HomeController(ILogger<HomeController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Login(string userName, string password)
        {
            if (userName == null || password == null)
            {
                return View();
            }
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName.ToLower() && u.Password == password.ToLower());
            if (user == null)
            {
                return View(new User
                {
                    UserName = userName,
                    Password = password
                });
            }

            SetCookie(userName);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        public  IActionResult Register(string userName, string password)
        {
            if (userName == null || password == null)
            {
                return View();
            }
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName.ToLower());
            if (user != null)
            {
                return View(new User
                {
                    UserName = userName,
                    Password = password
                });
            }
            _context.Users.Add(new User
            {
                UserName=userName.ToLower(),
                Password=password.ToLower(),
                IsAdmin = false,
            });
            SetCookie(userName);
            return RedirectToAction("Index");
        }

        private void SetCookie(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userName)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties
            {
                IsPersistent = true,
            };
            HttpContext.SignInAsync(principal, properties);
        }












        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}