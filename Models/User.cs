using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace NatterLite.Models
{
    public class User:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Country { get; set; }
        public byte[] ProfilePicture { get; set; }
        public byte[] BackgroundPicture { get; set; }
        public List<Chat> Chats { get; set; } 
        public List<User> BlackList { get; set; } 
        public string Status { get; set; }
        public bool IsBanned { get; set; }
        public User()
        {
            Chats = new List<Chat>();
            BlackList = new List<User>();
        }

    }
}
