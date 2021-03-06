﻿using ChatApp.Data;
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
        [HttpPost("[action]/{connectionId}/{roomId}")]
        public async Task<IActionResult> JoinRoom(string connectionId,string roomId)
        {
            await _hub.Groups.AddToGroupAsync(connectionId, roomId);
            return Ok();
        }
        [HttpPost("[action]/{connectionId}/{roomId}")]
        public async Task<IActionResult> LeaveRoom(string connectionId, string roomId)
        {
            await _hub.Groups.RemoveFromGroupAsync(connectionId, roomId);
            return Ok();
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage(int roomId, string message, [FromServices] AppDbContext context)
        {
            Message Message = new Message
            {
                Text = message,
                ChatId = roomId,
                Name = User.Identity.Name.Split(new char[] { '@' }).FirstOrDefault(),
                TimeStamp = DateTime.Now
            };
            await context.Messages.AddAsync(Message);
            await context.SaveChangesAsync();
            await _hub.Clients.Group(roomId.ToString()).SendAsync("RecieveMessage", new {
                Text=Message.Text,
                ChatId=Message.ChatId,
                Name=Message.Name,
                TimeStamp=Message.TimeStamp.ToString("dd/MM/yyyy HH:mm:ss")
            });
            return Ok();
        }
    }
}
