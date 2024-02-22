using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatVrite
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }

        public int UnreadMessagesCount { get; set; } 
    }
}
