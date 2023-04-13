using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineChatAPI
{
    public delegate void ChatMessageHandler(string message, ChatType chatType);
    public delegate void DisconnectedHandler(string reason);
    public delegate void PlayerOnlineHandler(string name, bool online, string uuid);
    public delegate void KeepAliveHandler();
    public delegate void ConnectedHandler();
    public delegate void ConnectErrorHandler(string reason);
    public delegate void UpdateHealthHandler(float health, int food);

    public interface IPacket
    {
        event ChatMessageHandler ChatMessage;
        event DisconnectedHandler Disconnected;
        event PlayerOnlineHandler PlayerOnline;
        event KeepAliveHandler KeepAlive;
        event ConnectedHandler Connected;
        event ConnectErrorHandler ConnectError;
        event UpdateHealthHandler UpdateHealth;

        void SendSpawn();
        void ProcessStream();
        void SendChat(string message);
        void Respawn();


    }
}


