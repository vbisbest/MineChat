using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineChatAPI
{
    public class Account
    {
        public Account()
        {
            UserName = string.Empty;
            Password = string.Empty;
            AccessToken = string.Empty;
            ClientToken = string.Empty;
            ProfileID = string.Empty;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Selected { get; set; }
        public string AccessToken { get; set; }
        public string ClientToken { get; set; }
        public string Skin { get; set; }
        public string PlayerName { get; set; }
        public string ProfileID { get; set; }
        public bool IsOffline { get; set; }
    }
}
