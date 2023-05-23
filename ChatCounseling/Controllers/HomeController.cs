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
            var user = _context.Users.FirstOrDefault(x => x.UserName == userName);

            if (user != null)
            {
                var userToChatRoom = _context.UserToChatRooms.FirstOrDefault(cr => cr.UserId == user.UserId);
                var chatRoom = _context.ChatRooms.FirstOrDefault(c => c.ChatRoomId == userToChatRoom.ChatRoomId);
                return View(chatRoom.Messages);
            }
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        public IActionResult ChatRoomForAdmin(int chatRoomId)
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.First(x => x.UserName == userName);
            if (user != null && user.IsAdmin)
            {
                var chatRoom = _context.ChatRooms.FirstOrDefault(cr => cr.ChatRoomId == chatRoomId);

                return View(nameof(ChatRoom), chatRoom.Messages);
            }
            return RedirectToAction(nameof(ChatRoom));
        }

        [Authorize]
        public IActionResult ChatRooms()
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.First(x => x.UserName == userName);
            if (user != null && user.IsAdmin)
            {
                var chatRooms = _context.ChatRooms.ToList();
                return View(chatRooms);
            }

            return RedirectToAction(nameof(Login));
        }

        [Route(nameof(Login))]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route(nameof(Login))]
        public IActionResult Login(string userName, string password)
        {
            if (userName == null || password == null)
            {
                return View();
            }
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName.ToLower() && u.Password == password.ToLower());
            if (user == null)
            {
                ModelState.AddModelError("UserName", "User is not exit");
                return View(new User
                {
                    UserName = userName,
                    Password = password
                });
            }

            SetCookie(userName);
            if (user.IsAdmin)
            {
                return RedirectToAction(nameof(ChatRooms));
            }
            return RedirectToAction(nameof(ChatRoom));

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
                ModelState.AddModelError("UserName", "this User is exit");
                return View(new User
                {
                    UserName = userName,
                    Password = password
                });
            }
            var newUser = new User
            {
                UserName = userName.ToLower(),
                Password = password.ToLower(),
                IsAdmin = false,
            };

            _context.Users.Add(newUser);
            
            _context.SaveChanges();
            
            var userid = _context.Users.FirstOrDefault(u => u.UserName == newUser.UserName && u.Password == newUser.Password).UserId;

            var newChaRoom = new Models.ChatRoom()
            {
                Creator = newUser.UserName,
            };

            _context.ChatRooms.Add(newChaRoom);
            
            _context.SaveChanges();

            var chatRoomId = _context.ChatRooms.FirstOrDefault(c => c.Creator == newUser.UserName).ChatRoomId;

            _context.UserToChatRooms.Add(new UserToChatRoom()
            {
                UserId = userid,
                ChatRoomId = chatRoomId
            });
            _context.SaveChanges();

            SetCookie(userName);
            
            return RedirectToAction(nameof(ChatRoom));
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

        [Route("Logout")]
        public IActionResult logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }










        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}