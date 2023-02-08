using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Chat Chat { get; set; }
        public string SenderUserName { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}
