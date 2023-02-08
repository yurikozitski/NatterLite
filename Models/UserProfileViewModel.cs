using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Models
{
    public class UserProfileViewModel
    {
        public string UserFullName { get; set; }
        public string UserUniqueName { get; set; }
        public string UserStatus { get; set; }
        public string UserDateOfBirth { get; set; }
        public string UserCountry { get; set; }
        public byte[] UserProfilePicture { get; set; }
        public byte[] UserBackgroundPicture { get; set; }
        public bool IsThisCurrentUser { get; set; }
    }
}
