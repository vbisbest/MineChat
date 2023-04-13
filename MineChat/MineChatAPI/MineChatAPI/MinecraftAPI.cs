using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MineChatAPI
{
    public class MinecraftAPI
    {
        public delegate void ChatMessageHandler(string message, ChatType chatType);
        public delegate void StatusMessageHandler(string statusMessage);
        public delegate void PlayerOnlineHandler(string name, bool online, string uuid);
        public delegate void ConnectionErrrorHandler(string message);
        public delegate void ConnectedHandler();
        public delegate void DisconnectedHandler(string reason);
        public delegate void KeepAliveHandler();
        public delegate void ServerInfoHandler(Server server);
        public delegate void ServerInfoErrorHandler(Server server, string message);
        public delegate void AuthenticatedHandler();
        public delegate void UpdateHealthHandler(int health, int food);

        public event ChatMessageHandler ChatMessage;
        public event StatusMessageHandler StatusMessage;
        public event PlayerOnlineHandler PlayerOnline;
        public event ConnectionErrrorHandler ConnectionError;
        public event ConnectedHandler Connected;
        public event DisconnectedHandler Disconnected;
        public event KeepAliveHandler KeepAlive;
        public event AuthenticatedHandler Authenticated;
        public event UpdateHealthHandler UpdateHealth;

        private const string LoginUrl = "https://login.minecraft.net?user={0}&password={1}&version=13";
        private const string ResourceUrl = "http://s3.amazonaws.com/MinecraftResources/";
        private const string DownloadUrl = "http://s3.amazonaws.com/MinecraftDownload/";

        bool disconnected = false;
        string disconnectReason = string.Empty;
        private Server currentServer;
        NetworkStream streamSocket = null;
        Socket socket = null;

        Account currentAccount = null;

        IPacket packet = null;
        MojangAPI mojangAPI = new MojangAPI();

        public MinecraftAPI()
        {
        }

        public static void AuthenticateAccount(Account account, bool reauth)
        {
            if (account == null)
            {
                throw new Exception("No account specified.  Create and select accounts on the Accounts page from the menu.");
            }
            
            try
            {
                if (!reauth)
                {
                    Debug.WriteLine("Begin authentication validate");
                    var validate = MojangAPIV2.Validate(account.AccessToken, account.ClientToken).Result;

                    if (validate.Result == RequestResult.Success)
                    {
                        Debug.WriteLine("Validate succeded");
                        return;
                    }

                    Debug.WriteLine("Begin refresh");
                    AuthResponse refreshed = MojangAPIV2.Refresh(account.AccessToken, account.ClientToken, account.ProfileID, account.PlayerName).Result;
                    if (refreshed.Result == RequestResult.Success)
                    {
                        Debug.WriteLine("refresh succeded");
                        account.AccessToken = refreshed.accessToken;
                        account.ProfileID = refreshed.selectedProfile.id;
                        account.PlayerName = refreshed.selectedProfile.name;
                        account.ClientToken = refreshed.clientToken;
                        return;
                    }
                }

                Debug.WriteLine("Begin authenticate");
                AuthResponse authenticate = MojangAPIV2.Authenticate(account.UserName, account.Password, account.ClientToken).Result;
                if (authenticate.Result == RequestResult.Success)
                {
                    Debug.WriteLine("Authenticate succeded");
                    account.AccessToken = authenticate.accessToken;
                    account.ProfileID = authenticate.selectedProfile.id;
                    account.PlayerName = authenticate.selectedProfile.name;
                    account.ClientToken = authenticate.clientToken;
                    return;
                }
                else
                {
                    Debug.WriteLine("Authentication failed");
                    throw new Exception(authenticate.Message);
                }
            }
            catch(Exception ex)
            {
                throw ex.GetBaseException();
            }
        }

        private void Packet_ConnectedError (string reason)
        {
            this.disconnectReason = reason;
            CloseSocket();
        }

        private void CloseSocket()
        {
            if (streamSocket != null)
            {
                try
                {

                    streamSocket.Close();
                    streamSocket.Dispose();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void Disconnect()
        {
            Debug.WriteLine("Disconnect called");

            disconnectReason = string.Empty;
            disconnected = true;
        }

        private void ThrowDisconnected()
        {            
            Debug.WriteLine("Throw Disconnected");

            if (Disconnected != null)
            {
                if(disconnectReason.Contains("Error creating session") && currentServer.UseOffline)
                {
                    disconnectReason = "This is not an offline server.  You must use a valid Minecraft account to connect to this server.";
                }

                string message = disconnectReason;
                Disconnected(message);
            }
        }

        private void ThrowConnectionError(string message)
        {

            Debug.WriteLine("Throw connection error");

            disconnectReason = string.Empty;
            disconnected = true;

            if (ConnectionError != null)
            {
                ConnectionError(message);
            }
        }

        private void ShowStatus(string statusMessage)
        {
            if (StatusMessage != null)
            {
                StatusMessage(statusMessage);
            }
        }

        public void EstablishConnection(Server server, Settings settings)
        {
            try
            {
                Debug.WriteLine("Establish connection protocol " + server.Protocol.ToString());

                Global.CurrentSettings = settings;
                Global.CurrentProtocol = server.Protocol;

                currentAccount = Global.CurrentSettings.GetSelectedAccount();
                if (currentAccount == null)
                {
                    throw new Exception("Default account not found");
                }

                this.disconnectReason = string.Empty;
                Global.IsOffline = server.UseOffline;
                this.currentServer = server;

                if (currentServer.UseOffline)
                {
                    if (!currentAccount.IsOffline)
                    {
                        throw new Exception("You must create or select an Offline account in the Accounts page to connect to Offline/Cracked servers");
                    }

                    Debug.WriteLine("Connecting offline");
                    StartConnection();
                }
                else if (currentAccount.IsOffline && !currentServer.UseOffline)
                {
                    throw new Exception("You are tyring to connect to an online server with an offline account. Either change your account to a valid Minecraft account (use email address) or connect to an offline server");
                }
                else
                {
                    Task.Run(() =>
                    {
                        Debug.WriteLine("Validate authentication");
                        try
                        {
                            MinecraftAPI.AuthenticateAccount(currentAccount, false);
                            if (Authenticated != null)
                            {
                                Authenticated();
                            }

                            StartConnection();
                        }
                        catch(Exception ex)
                        {
                            disconnectReason = "Failed to authenticate.  Make sure you are using an email addres for user name. - " + ex.GetBaseException().Message;
                            ThrowDisconnected();
                        }
                    });
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void ConnectTimeout(object state)
        {
            SocketAsyncEventArgs socketArgs = (SocketAsyncEventArgs)state;
            Socket.CancelConnectAsync(socketArgs);
        }

        private async void StartConnection()
        {
            try
            {
                Console.WriteLine("Starting connection " + currentServer.FullAddress + " - Protocol " + currentServer.Protocol.ToString());

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(currentServer.RealIP), currentServer.RealPort);
                SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();
                socketArgs.RemoteEndPoint = ipEndPoint;

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketArgs.Completed += SocketArgs_Completed;

                Timer t = new Timer(ConnectTimeout, socketArgs, 7000, 0);

                // Connect to the server
                bool result = socket.ConnectAsync(socketArgs);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("StartConnection error - " + ex.Message);
                disconnectReason = ex.Message;
                ThrowDisconnected();
            }
        }

        private void SocketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                MineStream mineStream = new MineStream(new NetworkStream((Socket)sender));

                // Send login request
                StatusLogin(mineStream, true);
                OnBeginRead(mineStream);
            }
        }

        public void StatusLogin(MineStream stream, bool login)
        {
            try
            {
                // Version 1.6
                if(currentServer.ProtocolVersion == ProtocolVersion.V3 && currentServer.ServerVersion == 0)
                {
                    List<byte> handshakeRequest = new List<byte>();
                    handshakeRequest.Add(Convert.ToByte(0x02));
                    handshakeRequest.Add(Convert.ToByte(currentServer.Protocol));
                    handshakeRequest.AddRange(BitConverter.GetBytes((short)currentAccount.PlayerName.Length).Reverse());
                    handshakeRequest.AddRange(Encoding.BigEndianUnicode.GetBytes(currentAccount.PlayerName));
                    handshakeRequest.AddRange(BitConverter.GetBytes((short)currentServer.Address.Length).Reverse());
                    handshakeRequest.AddRange(Encoding.BigEndianUnicode.GetBytes(currentServer.Address));
                    handshakeRequest.AddRange(BitConverter.GetBytes(Convert.ToInt32(currentServer.Port)).Reverse());

                    stream.Write(handshakeRequest.ToArray(), 0, handshakeRequest.Count);

                    return;
                }

                // Version 1.7 and higher
                List<byte> handshake = new List<byte>();

                handshake.Add(0x00);
                handshake.AddRange(WriteRawVarint32(currentServer.Protocol));
                handshake.AddRange(WriteRawVarint32(currentServer.Address.Length));
                handshake.AddRange(Encoding.UTF8.GetBytes(currentServer.Address));
                handshake.AddRange(BitConverter.GetBytes(Convert.ToUInt16(currentServer.RealPort)).Reverse());

                if (login)
                {
                    handshake.Add(0X02);
                }
                else
                {
                    handshake.Add(0x01);

                }

                handshake.InsertRange(0, WriteRawVarint32(handshake.Count));
                stream.Write(handshake.ToArray(), 0, handshake.Count);
                handshake.Clear();

                if (login)
                {
                    handshake.Add(0x00);                        
                    handshake.AddRange(WriteRawVarint32(currentAccount.PlayerName.Length));
                    handshake.AddRange(Encoding.UTF8.GetBytes(currentAccount.PlayerName));
                    handshake.InsertRange(0, WriteRawVarint32(handshake.Count));
                }
                else
                {
                    handshake.AddRange(WriteRawVarint32(1));
                    handshake.AddRange(WriteRawVarint32(0));
                }

                stream.Write(handshake.ToArray(), 0, handshake.Count);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        private void OnBeginRead(MineStream mineStream)
        {
            if (currentServer.ProtocolVersion == ProtocolVersion.V3 && currentServer.ServerVersion == 0)
            {
                Debug.WriteLine("Create packet V3");
                packet = new PacketV3(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 755)
            {
                Debug.WriteLine("Create packet V15");
                packet = new PacketV15(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 755)
            {
                Debug.WriteLine("Create packet V15");
                packet = new PacketV15(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 754)
            {
                Debug.WriteLine("Create packet V14");
                packet = new PacketV14(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 736)
            {
                Debug.WriteLine("Create packet V13");
                packet = new PacketV13(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 735 )
            {
                Debug.WriteLine("Create packet V12");
                packet = new PacketV12(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 577)
            {
                Debug.WriteLine("Create packet V11");
                packet = new PacketV11(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 498)
            {
                Debug.WriteLine("Create packet V10");
                packet = new PacketV10(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 404)
            {
                Debug.WriteLine("Create packet V9");
                packet = new PacketV9(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 342)
            {
                Debug.WriteLine("Create packet V8");
                packet = new PacketV8(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 335)
            {
                Debug.WriteLine("Create packet V7");
                packet = new PacketV7(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 319)
            {
                Debug.WriteLine("Create packet V6");
                packet = new PacketV6(mineStream);
            }
            else if (currentServer.ProtocolVersion == ProtocolVersion.V4 && currentServer.Protocol > 76)
            {
                Debug.WriteLine("Create packet V5");
                packet = new PacketV5(mineStream);
            }
            else
            {
                Debug.WriteLine("Create packet V4");
                packet = new PacketV4(mineStream);
            }

            packet.ChatMessage += packet_ChatMessage;
            packet.PlayerOnline += packet_PlayerOnline;
            packet.Disconnected += packet_Disconnected;
            packet.Connected += Packet_Connected;
            packet.ConnectError += Packet_ConnectedError;
            packet.UpdateHealth += Packet_UpdateHealth;


            packet.KeepAlive += delegate
            {
                if (KeepAlive != null)
                {
                    KeepAlive();
                }
            };

            Debug.WriteLine("Starting read loop");

            try
            {
                while (!disconnected)
                {
                    //Debug.WriteLine("Read Loop");

                    if (mineStream == null)
                    {
                        disconnected = true;
                    }
                    else
                    {
                        try
                        {
                            packet.ProcessStream();
                        }
                        catch (Exception ex)
                        {
                            // probably got disconnected
                            if(disconnectReason == string.Empty)
                            {
                                Debug.WriteLine("Read loop error. " + ex.Message);
                                disconnectReason = "Protocol error - " + ex.Message;                    
                            }

                            disconnected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Socket error: " + ex.Message);
                disconnectReason = ex.Message;
            }

            Debug.WriteLine("Exit read loop");

            mineStream.Disconnect();
            ThrowDisconnected();
        }

        private void Packet_UpdateHealth(float health, int food)
        {
            if(UpdateHealth != null)
            {

                UpdateHealth((int)Math.Floor(health), food);
            }
        }
        
        void Packet_Connected ()
        {
            if (Connected != null)
            {
                Connected();
            }
        }

        void packet_Disconnected(string reason)
        {
            disconnectReason = reason;
            disconnected = true;
            //ThrowDisconnected();
        }

        void packet_PlayerOnline(string name, bool online, string uuid)
        {
            if(name.Contains("?tab"))
            {
                return;
            }

            if (name.Contains("tab#"))
            {
                return;
            }

            if (PlayerOnline != null)
            {
                PlayerOnline(name, online, uuid);
            }
        }

        void packet_ChatMessage(string message, ChatType chatType)
        {
            if (ChatMessage != null)
            {
                ChatMessage(message, chatType);
            }
        }

        public void SendChat(string message)
        {
            if (packet != null)
            {
                packet.SendChat(message);
            }
        }

        public void SendSpawn()
        {
            if (packet != null)
            {
                packet.SendSpawn();

            }
        }

        public void Respawn()
        {
            if (packet != null)
            {
                packet.Respawn();
            }
        }

        private byte[] WriteRawVarint32(int value)
        {
            List<byte> b = new List<byte>();

            uint num = (uint)value;

            while (num >= 128U)
            {
                b.Add(((byte)(num | 128U)));
                num >>= 7;
            }

            b.Add(((byte)num));

            return b.ToArray();

        }
    }
}

 
