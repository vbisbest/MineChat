using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace MineChatAPI
{
    public class ServerInfo2
    {
        public delegate void ServerInfoHandler(Server server);
        public event ServerInfoHandler ServerInfoResult;

        public delegate void ServerInfoErrorHandler(Server server, string message);
        public event ServerInfoErrorHandler ServerInfoError;

        private const string SERVER_ERROR_CANT_REACH = "Can't reach server";
        private const string SERVER_ERROR_NOT_SUPPORTED = "Not supported";
        private const string SERVER_ERROR_OUTDATED_SERVER = "Version not supported";
        private const string SERVER_ERROR_UNDERSTAND_SERVER = "Can't understand server";

        bool delayV3Ping = false;
        bool setReadTimeout = false;

        public ServerInfo2()
        {
        }

        public void GetServerInfo(Server server, bool delayV3Ping, bool setReadTimeout, bool isWindows)
        {
            try
            {
 
                //if (!server.Address.ToLower().Contains("fade"))
                if(!server.ServerName.ToLower().Contains("ams"))
                {
                   //return;
                }

                this.delayV3Ping = delayV3Ping;
                this.setReadTimeout = setReadTimeout;

                ServerInfoState2 serverInfoState = new ServerInfoState2();
                serverInfoState.isWindows = isWindows;
                serverInfoState.Server = server;
                serverInfoState.Server.ProtocolVersion = ProtocolVersion.V4;
                serverInfoState.Server.MOTD = string.Empty;
                serverInfoState.Server.FavIcon = null;
                serverInfoState.Server.Status = ServerStatus.Polling;

                server.Protocol = -1;
                SetProtocol(server);              

                ConnectToServer(serverInfoState);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task ConnectToServer(ServerInfoState2 serverInfoState)
        {
            Server server = serverInfoState.Server;

            if (serverInfoState.Server.RealPort == 0)
            {
                serverInfoState.Server.RealPort = 25565;
            }

            try
            {
                // Create connection
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(server.RealIP), server.RealPort);
                SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();
                socketArgs.RemoteEndPoint = ipEndPoint;
                socketArgs.UserToken = serverInfoState;

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketArgs.Completed += SocketArgs_Completed;

                // Setup timeout
                Timer t = new Timer(ConnectTimeout, socketArgs, 7000, 0);

                // Connect to the server
                bool result = socket.ConnectAsync(socketArgs);
            }
            catch(Exception ex)
            {
                server.Status = ServerStatus.Error;
                server.MOTD = SERVER_ERROR_CANT_REACH + ":" + ex.Message;
                if (ServerInfoError != null)
                {
                    ServerInfoError(server, ex.Message);
                }

                Console.WriteLine(ex.Message);
            }
        }

        private void ConnectTimeout(object state)
        {
            SocketAsyncEventArgs socketArgs = (SocketAsyncEventArgs)state;
            Socket.CancelConnectAsync(socketArgs);            
        }

        private void SocketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ServerInfoState2 serverInfoState = (ServerInfoState2)e.UserToken;

            if (e.SocketError == SocketError.Success)
            {                
                serverInfoState.Socket = new NetworkStream((Socket)sender);
                if (serverInfoState.Server.ProtocolVersion == ProtocolVersion.V4)
                {
                    if (!ProtocolV2Ping(serverInfoState))
                    {
                        serverInfoState.Server.ProtocolVersion = ProtocolVersion.V3;
                        ConnectToServer(serverInfoState);
                    }
                }
                else
                {
                    Debug.WriteLine("Ping v1");
                    ProtocolV1Ping(serverInfoState);
                }
            }
            else
            {
                serverInfoState.Server.Status = ServerStatus.Error;

                string error = SERVER_ERROR_CANT_REACH;
                string serv = serverInfoState.Server.RealIP + ":" + serverInfoState.Server.RealPort.ToString();
                serverInfoState.Server.MOTD = "Address " + serv + ": " + error;

                if (ServerInfoError != null)
                {
                    ServerInfoError(serverInfoState.Server, e.SocketError.ToString());
                }
            }
        }

        private void SetProtocol(Server server)
        {
            // Override server version based on user setting
            if (server.ServerVersion == 1)
            {
                server.Protocol = 5;
            }
            else if (server.ServerVersion == 2)
            {
                server.Protocol = 47;
            }
            else if (server.ServerVersion == 3)
            {
                server.Protocol = 107;
            }
            else if (server.ServerVersion == 4)
            {
                server.Protocol = 110;
            } else if (server.ServerVersion == 5) {
                server.Protocol = 210;
            }
            else if (server.ServerVersion == 6) {
                server.Protocol = 315;
            }
            else if (server.ServerVersion == 7)
            {
                server.Protocol = 316;
            }
            else if (server.ServerVersion == 8)
            {
                server.Protocol = 335; //1.12
            }
            else if (server.ServerVersion == 9)
            {
                server.Protocol = 338; //1.12.1
            }
            else if (server.ServerVersion == 10)
            {
                server.Protocol = 340; // 1.12.2
            }
            else if (server.ServerVersion == 11)
            {
                server.Protocol = 393; //1.13
            }
            else if (server.ServerVersion == 12)
            {
                server.Protocol = 401; //1.13.1
            }
            else if (server.ServerVersion == 13)
            {
                server.Protocol = 404; //1.13.2
            }
            else if (server.ServerVersion == 14)
            {
                server.Protocol = 477; //1.14
            }
            else if (server.ServerVersion == 15)
            {
                server.Protocol = 480; //1.14.1
            }
            else if (server.ServerVersion == 16)
            {
                server.Protocol = 485; //1.14.2
            }
            else if (server.ServerVersion == 17)
            {
                server.Protocol = 490; //1.14.3
            }
            else if (server.ServerVersion == 18)
            {
                server.Protocol = 498; //1.14.4
            }
            else if (server.ServerVersion == 19)
            {
                server.Protocol = 575; //Snapshot
            }
            else if (server.ServerVersion == 20)
            {
                server.Protocol = 578;
            }
            else if (server.ServerVersion == 21)
            {
                server.Protocol = 736;
            }
            else if (server.ServerVersion == 22)
            {
                server.Protocol = 744;
            }
            else if (server.ServerVersion == 23)
            {
                server.Protocol = 753;
            }
            else if (server.ServerVersion == 24)
            {
                server.Protocol = 754;
            }
            else if (server.ServerVersion == 26)
            {
                server.Protocol = 755;
            }
            else if (server.ServerVersion == 27)
            {
                server.Protocol = 756;
            }

        }

        private void ProtocolV1Ping(ServerInfoState2 serverInfoState)
        {
            byte[] buffer;
            List<byte> bufferList = new List<byte>();
            List<byte> currentMessage = new List<byte>();
            List<byte> currentChunk = new List<byte>();
            List<string> results = new List<string>();

            Server server = serverInfoState.Server;                    

            //ConnectToServer(serverInfoState);
            NetworkStream socket = serverInfoState.Socket;

            int packetLen = 0;

            try
            {
                List<byte> pingRequest = new List<byte>();

                string pingString = "MC|PingHost";
                pingRequest.Add(Convert.ToByte(0xFE));
                pingRequest.Add(Convert.ToByte(0x01));

                
                pingRequest.Add(Convert.ToByte(0xFA));

                byte[] ping = UnicodeEncoding.BigEndianUnicode.GetBytes(pingString);
                pingRequest.AddRange(BitConverter.GetBytes(Convert.ToUInt16(pingString.Length)).Reverse());
                pingRequest.AddRange(ping);
                //pingRequest.AddRange(Encoding.BigEndianUnicode.GetBytes(pingString));


                byte[] ping2 = UnicodeEncoding.BigEndianUnicode.GetBytes(server.Address);

                int count = 7 + (ping2.Length);
                pingRequest.AddRange(BitConverter.GetBytes(Convert.ToUInt16(count)).Reverse());

                pingRequest.Add(Convert.ToByte(0x4e));     
                //pingRequest.Add(Convert.ToByte(73));
                pingRequest.AddRange(BitConverter.GetBytes(Convert.ToUInt16(ping2.Length)).Reverse());
                pingRequest.AddRange(ping2);
                pingRequest.AddRange(BitConverter.GetBytes(Convert.ToInt32(server.RealPort)).Reverse());

                string st = string.Empty;

                foreach(byte cb in pingRequest)
                {
                    st += cb.ToString("X2") + " ";
                }
                
                // Send the ping request
                //Debug.WriteLine(st);
                socket.Write(pingRequest.ToArray(), 0, pingRequest.Count);

                PacketReader pr = new PacketReader(15000);
                pr.SetCurrentPacket(socket);

                // Get the response
                pr.ReadByte(); // always 255
                packetLen = pr.ReadShort() * 2;  
                buffer = new byte[packetLen];

                socket.Read(buffer, 0, packetLen);

                bufferList.AddRange(buffer);

                for(int currentPosition = 0; currentPosition < bufferList.Count; currentPosition+=2)
                {
                    currentChunk.AddRange(bufferList.GetRange(currentPosition, 2));
                    if((currentChunk.ElementAt(0) == 0x00 && currentChunk.ElementAt(1) == 0x00) || (currentPosition + 2) == bufferList.Count)
                    {
                        // end of message
                        string message = UnicodeEncoding.BigEndianUnicode.GetString(currentMessage.ToArray(), 0, currentMessage.Count);

                        //Console.WriteLine(message);
                        results.Add(message);
                        currentMessage.Clear();
                    }
                    else
                    {
                        currentMessage.AddRange(currentChunk);
                    }

                    currentChunk.Clear();
                }

                string players = results.ElementAt(4) + "/" + results.ElementAt(5);
                server.Info = players;
                server.MOTD = results.ElementAt(3);
                server.Protocol = Convert.ToInt32(results.ElementAt(1));
                if(server.Protocol == 127)
                {
                    server.ProtocolVersion = ProtocolVersion.V4;
                    server.Status = ServerStatus.OK;
                    server.Protocol = 210;
                    ServerInfoResult(server);
                }
                else
                {
                    server.ProtocolVersion = ProtocolVersion.V3;
                    SetProtocol(server);

                    if (ServerInfoResult != null)
                    {
                        server.Status = ServerStatus.OK;
                        ServerInfoResult(server);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(server.Address + ": " + ex.Message);
                server.Status = ServerStatus.Error;
                server.MOTD = SERVER_ERROR_UNDERSTAND_SERVER + ":" + ex.Message;
                if (ServerInfoError != null)
                {
                    ServerInfoError(server, ex.Message);
                }
            }
        }

        private bool ProtocolV2Ping(ServerInfoState2 serverInfoState)
        {
            NetworkStream socket = serverInfoState.Socket;
            Server server = serverInfoState.Server;

            int packetLen = 0;
            server.ProtocolVersion = ProtocolVersion.V4;

            try
            {
                PacketReader pr = new PacketReader(15000);

                // This is the new protocol, need to ping differently
                List<byte> handshake = new List<byte>();
                handshake.Add(0x00);
                handshake.AddRange(pr.WriteRawVarint32(server.Protocol));
                handshake.AddRange(pr.WriteRawVarint32(server.Address.Length));
                handshake.AddRange(Encoding.UTF8.GetBytes(server.Address));
                handshake.AddRange(BitConverter.GetBytes(Convert.ToUInt16(server.RealPort)).Reverse());
                handshake.Add(0x01);
                handshake.InsertRange(0, pr.WriteRawVarint32(handshake.Count));
                socket.Write(handshake.ToArray(), 0, handshake.Count);

                string hex = BitConverter.ToString(handshake.ToArray());

                // Part 2                
                handshake.Clear();
                handshake.AddRange(pr.WriteRawVarint32(1));
                handshake.AddRange(pr.WriteRawVarint32(0));

                socket.Write(handshake.ToArray(), 0, handshake.Count);
                pr.SetCurrentPacket(socket);

                List<byte> resp = new List<byte>();

                packetLen = pr.ReadRawVarint32();
                var packet = pr.ReadRawVarint32();
                var stringlen = pr.ReadRawVarint32();

                resp.AddRange(pr.ReadByteArray(stringlen));
                string pingResponse = Encoding.UTF8.GetString(resp.ToArray());
                PingResponse response = JsonConvert.DeserializeObject<PingResponse>(pingResponse);

                server.Protocol = response.version.protocol;
                server.Info = string.Format("{0}/{1}", response.players.online, response.players.max);
                server.MOTD = response.descriptionText;

                // Grab fav icon
                try
                {
                    if(response.favicon == null)
                    {
                        server.FavIcon = null;
                    }
                    else
                    {
                        string[] favSplit = response.favicon.Split(",".ToCharArray());
                        server.FavIcon = Convert.FromBase64String(favSplit[1]);
                    }
                }
                catch
                {
                    server.FavIcon = null;
                }
            }
            catch(Exception ex)
            {
                try
                {
                    socket.Close();
                    socket.Dispose();
                }
                catch(Exception e)
                {
                    Debug.WriteLine("error");
                    // ignore
                }

                return false;
            }

            SetProtocol(server);

            if (ServerInfoResult != null)
            {
                server.Status = ServerStatus.OK;
                ServerInfoResult(server);
            }

            return true;
        }
    }

    public class PingResponse
    {
        public JToken description { get; set; }

        public string descriptionText
        {
            get
            {
                try
                {
                    if (description.Type == JTokenType.String)
                    {
                        return description.ToString();
                    }
                    else
                    {
                        string s = description["text"].ToString();
                        if(s==string.Empty)
                        {
                            s = description["extra"].ToString();
                        }

                        return s;

                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public string favicon { get; set; }
        public version version { get; set; }
        public players players { get; set; }
    }

    public class version
    {
        public version()
        {           
        }
        public string name { get; set; }
        public int protocol { get; set; }
    }

    public class players
    {
        public players()
        {           
        }
        public int max { get; set; }
        public int online { get; set; }
    }


    public class description
    {
        public description()
        {           
        }
        public string text { get; set; }
    }


}

