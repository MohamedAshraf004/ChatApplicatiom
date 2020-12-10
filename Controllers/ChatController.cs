using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private  IHubContext<ChatHub> _hub;

        public ChatController(IHubContext<ChatHub> hub)
        {
            this._hub = hub;
        }
        [HttpPost("[action]/{connectionId}/{roomName}")]
        public async Task<IActionResult> JoinRoom(string connectionId,string roomName)
        {
            await _hub.Groups.AddToGroupAsync(connectionId, roomName);
            return Ok();
        }
        [HttpPost("[action]/{connectionId}/{roomName}")]
        public async Task<IActionResult> LeaveRoom(string connectionId, string roomName)
        {
            await _hub.Groups.RemoveFromGroupAsync(connectionId, roomName);
            return Ok();
        }

        public async Task<IActionResult> SendMessage(int chatId, string message,string roomName,[FromServices] AppDbContext context)
        {
            Messages msg = new Messages
            {
                Text = message,
                ChatId = chatId,
                Name = User.Identity.Name.Split(new char[] { '@' }).FirstOrDefault(),
                TimeStamp = DateTime.Now
            };
            await context.Messages.AddAsync(msg);
            await context.SaveChangesAsync();
            await _hub.Clients.Group(roomName).SendAsync("RecieveMessage",msg);
            return Ok();
        }
    }
}
