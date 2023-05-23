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
        public IActionResult ChatRoom()
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.First(x => x.UserName == userName);

            var chatRoom = _context.ChatRooms.FirstOrDefault(cr=>cr.Applicant.UserName == userName);
            if(chatRoom == null)
            {
                _context.ChatRooms.Add(new ChatRoom
                {
                    ApplicantId = user.UserId,
                    Applicant = user,
                    messages = new List<Message>(),
                });
            }

            return View(chatRoom.messages);
        }

        [Authorize]
        public IActionResult ChatRoom(int chatRoomId)
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.First(x => x.UserName == userName);
            if (user.IsAdmin)
            {
                var chatRoom = _context.ChatRooms.FirstOrDefault(cr => cr.ChatRoomId == chatRoomId);
                if (chatRoom == null)
                {
                    _context.ChatRooms.Add(new ChatRoom
                    {
                        ApplicantId = user.UserId,
                        Applicant = user,
                        messages = new List<Message>(),
                    });
                }

                return View(chatRoom.messages);
            }
            return RedirectToAction(nameof(ChatRoom));
        }

        [Authorize]
        public IActionResult Admin()
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.First(x => x.UserName == userName);
            var chatRoom = _context.ChatRooms.ToList();
            return View(chatRoom);
        }

        [Route("Login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route("Login")]
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
            if (user.IsAdmin) 
            {
                return RedirectToAction("Admin");
            }
            return RedirectToAction("ChatRoom");

        }

        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
        {
            return View();
        }
        [Route("Register")]
        public IActionResult Register(string userName, string password)
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
            return RedirectToAction("ChatRoom");
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