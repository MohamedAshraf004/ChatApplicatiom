using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _appDb;

        public HomeController(AppDbContext appDb)
        {
            this._appDb = appDb;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var chats = _appDb.Chats.Include(x => x.Users)
                    .Where(x => !x.Users.Any(y=>y.UserId==userId)).ToList();
            return View(chats);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            if (ModelState.IsValid)
            {
                var chat = new Chat
                {
                    Name = name,
                    Type = ChatType.Group
                };
                chat.Users.Add(
                    new ChatUser
                    {
                        UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                        Role = UserRole.Admin
                    });
                await _appDb.Chats.AddAsync(chat);
                await _appDb.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("{id}")]
        public IActionResult Chat(int id) => View(_appDb.Chats.Include(msg => msg.Messages).FirstOrDefault(x => x.Id == id));

        [HttpPost]
        public async Task<IActionResult> AddMessage(int chatId, string message)
        {
            if (ModelState.IsValid)
            {
                Messages msg = new Messages
                {
                    Text = message,
                    ChatId = chatId,
                    Name = User.Identity.Name.Split(new char[] { '@' }).FirstOrDefault(),
                    TimeStamp = DateTime.Now
                };
                await _appDb.Messages.AddAsync(msg);
                await _appDb.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Chat), new { id = chatId });
        }
        [HttpGet]
        public async Task<IActionResult> JoinRoom(int id)
        {

            var chatUser = new ChatUser
            {
                ChatId = id,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Member
            };
            await _appDb.ChatUsers.AddAsync(chatUser);
            await _appDb.SaveChangesAsync();
            return RedirectToAction(nameof(Chat), "Home", new { id = id });
        }
    }
}