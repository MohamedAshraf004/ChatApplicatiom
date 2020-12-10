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
            var chats = _appDb.Chats.Include(x => x.Users).Include(x => x.Messages)
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

        public IActionResult Chat(int id)
        {
            return View(_appDb.Chats.Include(msg => msg.Messages).FirstOrDefault(x => x.Id == id));
        }

        public IActionResult Find()
        {
            var users = _appDb.Users.Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value).ToList();
            return View(users);
        }

        public IActionResult PrivateChats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View(_appDb.Chats.Include(x => x.Users).ThenInclude(x=>x.User).Where(x => x.Users.Any(y=>y.UserId == userId)  && x.Type == ChatType.Private).ToList());
        }

        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {
            var chat = new Chat
            {
                Type = ChatType.Private
            };
            chat.Users.Add(new ChatUser
            {
                UserId = userId
            });
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value
            });
            await _appDb.Chats.AddAsync(chat);
            await _appDb.SaveChangesAsync();

            return RedirectToAction(nameof(Chat), new { id = chat.Id });
        }

        [HttpPost]
        public async Task<IActionResult> AddMessage(int chatId, string message)
        {
            if (ModelState.IsValid)
            {
                Message msg = new Message
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