using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineChatAPI
{

    public enum CommandType
    {
        Player,
        Server
    }

    public class Command
    {
        public string CommandText { get; set; }
        public CommandType Type { get; set; }

        public string TypeString
        {
            get
            {
                if (this.Type == CommandType.Player)
                {
                    return "Player Commands";
                }
                else
                {
                    return "Server Commands";
                }
            }
        }
    }
}
