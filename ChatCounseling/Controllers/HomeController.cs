using ChatCounseling.Data;
using ChatCounseling.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace ChatCounseling.Controllers
{
//    [Route("[action]")]
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
        public async Task<IActionResult> ChatRoom()
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }
            if (user.IsAdmin)
            {
                return RedirectToAction(nameof(ChatRooms));
            }
            var userToChatRoom = await _context.UserToChatRooms.FirstOrDefaultAsync(cr => cr.UserId == user.UserId);

            if (userToChatRoom == null)
            {
                return RedirectToAction(nameof(Login));
            }
            var chatRoom = await _context.ChatRooms.FirstOrDefaultAsync(c => c.ChatRoomId == userToChatRoom.ChatRoomId);
            if (chatRoom == null)
            {
                return RedirectToAction(nameof(Login));
            }
            var messages = await _context.Messages.Include(m => m.User).Where(x => x.ChatRoomId == chatRoom.ChatRoomId).ToListAsync();
            ViewBag.ChatRoomId = chatRoom.ChatRoomId;
            return View(messages);

        }

        [Authorize]
        public async Task<IActionResult> ChatRoomForAdmin(int chatRoomId)
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user != null && user.IsAdmin)
            {
                var chatRoom = await _context.ChatRooms.FirstOrDefaultAsync(cr => cr.ChatRoomId == chatRoomId);
                if (chatRoom == null)
                {
                    return RedirectToAction(nameof(ChatRooms));
                }
                var messages = await _context.Messages.Include(m=>m.User).Where(x => x.ChatRoomId == chatRoom.ChatRoomId).ToListAsync();
                ViewBag.ChatRoomId = chatRoom.ChatRoomId;
                return View(nameof(ChatRoom), messages);
            }
            return RedirectToAction(nameof(ChatRoom));
        }

        [Authorize]
        public async Task<IActionResult> ChatRooms()
        {
            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user != null && user.IsAdmin)
            {
                var chatRooms = await _context.ChatRooms.ToListAsync();
                return View(chatRooms);
            }

            return RedirectToAction(nameof(Login));
        }
        #region login logout cookie register
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [Route(nameof(Login))]
        public async Task<IActionResult> Login(string userName, string password)
        {
            if (userName == null || password == null)
            {
                return View();
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName.ToLower() && u.Password == password.ToLower());
            if (user == null)
            {
                ModelState.AddModelError("UserName", "User is not exit");
                return View(new User
                {
                    UserName = userName,
                    Password = password
                });
            }

            await SetCookie(userName);
            if (user.IsAdmin)
            {
                return RedirectToAction(nameof(ChatRooms));
            }
            return RedirectToAction(nameof(ChatRoom));

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        
        public async Task<IActionResult> Register(string userName, string password)
        {
            if (userName == null || password == null)
            {
                return View();
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName.ToLower());
            if (user != null)
            {
                ModelState.AddModelError("UserName", "this User is exit");
                return View(new User
                {
                    UserName = userName,
                    Password = password
                });
            }
            var newUserModel = new User
            {
                UserName = userName.ToLower(),
                Password = password.ToLower(),
                IsAdmin = false,
            };

            _context.Users.Add(newUserModel);
            
            _context.SaveChanges();
            
            var newUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == newUserModel.UserName && u.Password == newUserModel.Password);
            var userId = newUser.UserId;
            var newChaRoom = new Models.ChatRoom()
            {
                Creator = newUser.UserName,
            };

            await _context.ChatRooms.AddAsync(newChaRoom);
            
            await _context.SaveChangesAsync();

            var chatRoom = await _context.ChatRooms.FirstOrDefaultAsync(c => c.Creator == newUser.UserName);
            var chatRoomId = chatRoom.ChatRoomId;
            _context.UserToChatRooms.Add(new UserToChatRoom()
            {
                UserId = userId,
                ChatRoomId = chatRoomId
            });
            await _context.SaveChangesAsync();

            await SetCookie(userName);
            
            return RedirectToAction(nameof(ChatRoom));
        }

        private async Task SetCookie(string userName)
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
            await HttpContext.SignInAsync(principal, properties);
        }

        public async Task<IActionResult> logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
        #endregion

        [Authorize]
        public async Task<IActionResult> SendMessage(string body,int chatRoomId)
        {

            var userName = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            var chatRoom = await _context.ChatRooms.FirstOrDefaultAsync(c => c.ChatRoomId == chatRoomId);
            if (user == null || chatRoom == null )
            {
                return RedirectToAction("Login");
            }
            if (!user.IsAdmin)
            {
                if(user.UserName != chatRoom.Creator)
                {
                    return RedirectToAction("Login");
                }
            }
            var message = new Message()
            {
                Body = body,
                User = user,
                UserId = user.UserId,
                ChatRoom=chatRoom,
                ChatRoomId=chatRoomId,
            };
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
            if (user.IsAdmin)
            {
                return RedirectToAction(nameof(ChatRoomForAdmin),new { chatRoomId });
            }
            return RedirectToAction(nameof(ChatRoom));
        }






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}