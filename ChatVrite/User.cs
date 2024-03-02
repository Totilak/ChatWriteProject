using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ChatVrite
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime DateOfRegistration { get; set; }  
        public DateTime Birthday { get; set; }
        public string City { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; }
        public int RequestsID { get; set; }
        public SolidColorBrush ColorUser { get; set; }

        public int UnreadMessagesCount { get; set; }



    }
}
