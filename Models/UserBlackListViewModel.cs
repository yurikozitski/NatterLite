using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Models
{
    public class UserBlackListViewModel
    {
        public string UserName { get; set; }
        public string UserUniqueName { get; set; }
        public byte[] UserProfilePicture { get; set; }
    }
}
