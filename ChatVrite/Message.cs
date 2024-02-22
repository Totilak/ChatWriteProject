using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatVrite
{
    public class Message
    {
        public int SenderUserID { get; set; }
        public int ReceiverUserID { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }

        public bool IsRead { get; set; }
    }

}
