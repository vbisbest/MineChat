using System;
//using Sockets.Plugin;
using System.Net.Sockets;

namespace MineChatAPI
{
    public class ServerInfoState2
    {
        public NetworkStream Socket;
        public Server Server;
        public bool isWindows { get; set; }
        public ServerInfoState2()
        {
        }
    }
}

