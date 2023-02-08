using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Models
{
    public class ChatViewModel
    {
        public string ChatId { get; set; }
        public string LastMessageText { get; set; }
        public DateTime LastMessageTime { get; set; }
        public string UnreadMessagesCount { get; set; }
        public string CompanionUserName { get; set; }
        public byte[] CompanionUserProfilePicture { get; set; }
        public DateTime TimeForCompare { get; set; }
    }
}
