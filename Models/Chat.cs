using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Models
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string CreatorUserName{ get; set; }
        public DateTime CreationTime { get; set; }
        public string LastVisitedBy { get; set; }
        public List<User> Users { get; set; } 
        public List<Message> Messages { get; set; }
        public Chat()
        {
            Users = new List<User>();
            Messages = new List<Message>();
        }
    }
}
