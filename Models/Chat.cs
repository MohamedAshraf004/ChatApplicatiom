﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class Chat
    {
        public Chat()
        {
            Messages=new List<Message>() { };
            Users = new List<ChatUser>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Message> Messages { get; set; }
        public ICollection<ChatUser> Users{ get; set; }
        public ChatType Type { get; set; }
    }
}
