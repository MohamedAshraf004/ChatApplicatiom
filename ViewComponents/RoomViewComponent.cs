using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ViewComponents
{
    public class RoomViewComponent:ViewComponent
    {
        private readonly AppDbContext appDb;

        public RoomViewComponent(AppDbContext appDb)
        {
            this.appDb = appDb;
        }
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View(appDb.ChatUsers.Include(x=>x.Chat).Where(x=>x.UserId==userId && x.Chat.Type==ChatType.Group).Select(x=>x.Chat).ToList());
        }

    }
}
