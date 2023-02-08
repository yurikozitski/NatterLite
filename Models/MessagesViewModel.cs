using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Models
{
    public class MessagesViewModel
    {
        public string CompanionUserIdentityName { get; set; }
        public string CompanionUserName { get; set; }
        public string CompanionUserStatus { get; set; }
        public byte[] CompanionUserProfilePicture { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
        public bool DidCurrentUserAddedCompanionUserToBlackList { get; set; }
        public bool DidCompanionUserAddedCurrentUserToBlackList { get; set; }
    }
}
