using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Models
{
    public class UserSearchViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserUniqueName { get; set; }
        public byte[] UserProfilePicture { get; set; }
        public bool IsBanned { get; set; }
    }
}
