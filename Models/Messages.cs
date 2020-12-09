using System;

namespace ChatApp.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}