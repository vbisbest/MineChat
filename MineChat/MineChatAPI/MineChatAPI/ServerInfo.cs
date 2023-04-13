using System;
using System.IO;
using System.Net;
using System.Threading;
//using System.Net.Sockets;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using System.Text;
//using Heijden.DNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;

using System.Diagnostics;

//using System.Json;

namespace MineChatAPI
{
    public class ServerInfo
    {
		public delegate void ServerInfoHandler(Server server);
		public event ServerInfoHandler ServerInfoResult;

		public delegate void ServerInfoErrorHandler(Server server, string message);
		public event ServerInfoErrorHandler ServerInfoError;

		private const string SERVER_ERROR_CANT_REACH = "Can't reach server";
		private const string SERVER_ERROR_NOT_SUPPORTED = "Not supported";
		private const string SERVER_ERROR_OUTDATED_SERVER = "Version not supported";
		private const string SERVER_ERROR_UNDERSTAND_SERVER = "Can't understand server";

		//private MinecraftStreamReader mcStreamReader = new MinecraftStreamReader();

        public ServerInfo()
        {

        }
        
		public async void GetServerInfo(Server server)
		{
            try
            {
                string host = "_minecraft._tcp." + server.Address + ".";


            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            

            //if(!server.ServerName.Contains("sup"))
            //{
                //return;
            //}
            /*
			ServerInfoState state = new ServerInfoState();
			state.Server = server;
            //Console.WriteLine(server.Address);

			ParameterizedThreadStart job = new ParameterizedThreadStart(DoGetServerInfo);
			Thread thread = new Thread(job);
			thread.Start(state);
             */
            

		}

        /*
        private void DoGetServerInfo(object serverState)
		{

			ServerInfoState state = (ServerInfoState)serverState;

			// Need to find the real IP of the server
			IPAddress realIP = null;
			int realPort = 0;

			try
			{
                if(GetRealIPAndPort (state.Server.Address, Convert.ToInt32(state.Server.Port), out realIP, out realPort))
                {
				    state.Server.RealIP = realIP.ToString();
                }
                else
                {
                    if(realIP == IPAddress.None)
                    {
                        throw new Exception("Error resolving");
                    }
                    else
                    {
                        state.Server.RealIP = state.Server.Address;
                    }
                }
				state.Server.RealPort = realPort;
			}
            catch(Exception ex)
			{
				//Debug.WriteLine("Could not resolve " + state.Server.Address);
				if(ServerInfoError != null)
				{
                    state.Server.Info = ex.Message;
                    state.Server.Status = ServerStatus.Error;
					ServerInfoError(state.Server, SERVER_ERROR_CANT_REACH);
				}

				return;
			}

			// Now connect to the server to get the info
			try
			{
				DoPing(state.Server);
			}
			catch(Exception ex)
			{
				Console.WriteLine("Cannot connect: " + ex.Message);
			}
		}

		public void DoPing(Server server)
		{
			try
			{
                ProtocolV1Ping(server);
                if(server.Protocol > 60 && server.Protocol < 127)
                {
                    return;
                }
                else
                {
				    ProtocolV2Ping(server);
                }
			}
			catch(Exception ex)
			{
				server.Info = ex.Message;
				server.Status = ServerStatus.Error;

                if (ServerInfoError != null)
                {
                    ServerInfoError(server, ex.Message);
                }
				return;
			}
            server.Status = ServerStatus.OK;
			ServerInfoResult(server);
		}

		public bool ProtocolV1Ping(Server server)
		{
			TcpClient client = null;

			try
			{
				client = GetTcpClient(server);
			}
			catch
			{
                //return false;
                throw new Exception(SERVER_ERROR_CANT_REACH);
			}

			byte[] buffer;
			List<byte> bufferList = new List<byte>();

			try
			{
				List<byte> pingRequest = new List<byte>();

				string pingString = "MC|PingHost";
				pingRequest.Add(Convert.ToByte(0xFE));
				pingRequest.Add(Convert.ToByte(0x01));
				pingRequest.Add(Convert.ToByte(0xFA));

				pingRequest.AddRange(BitConverter.GetBytes((short)pingString.Length).Reverse());
				pingRequest.AddRange(Encoding.BigEndianUnicode.GetBytes(pingString));
				int count = 7 + (2 * server.RealIP.Length);
				pingRequest.AddRange(BitConverter.GetBytes((short)count).Reverse());
                pingRequest.Add(Convert.ToByte(0x4e));
                //pingRequest.Add(Convert.ToByte(0x04));
				pingRequest.AddRange(BitConverter.GetBytes((short)server.RealIP.Length).Reverse());
				pingRequest.AddRange(Encoding.BigEndianUnicode.GetBytes(server.RealIP));
				pingRequest.AddRange(BitConverter.GetBytes(Convert.ToInt32(server.Port)).Reverse());

				client.Client.Send(pingRequest.ToArray());

				NetworkStream stream = client.GetStream();

				try
				{
					mcStreamReader.ReadByte(stream);
					mcStreamReader.ReadShort(stream);
				}
                catch(Exception ex)
				{
                    Console.WriteLine(ex.Message);
                    server.Protocol = 127;
					return false;
				}

				buffer = new byte[1024];

				int result = 1;

				try
				{
					// Get the response
					while(result != 0)
					{
						result = client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
						bufferList.AddRange(buffer.Take(result));
					}
				}
				catch
				{
					// Do nothing, got all the bytes
				}

				if(client.Connected)
				{
					client.Close();
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Recieve fail: " + ex.Message);
				throw new Exception(SERVER_ERROR_UNDERSTAND_SERVER + ": " + ex);
			}

			List<byte> currentMessage = new List<byte>();
			List<byte> currentChunk = new List<byte>();
			List<string> results = new List<string>();

			for(int currentPosition = 0; currentPosition < bufferList.Count; currentPosition+=2)
			{
				currentChunk.AddRange(bufferList.GetRange(currentPosition, 2));
				if((currentChunk.ElementAt(0) == 0x00 && currentChunk.ElementAt(1) == 0x00) || (currentPosition + 2) == bufferList.Count)
				{
					// end of message
					string message = UnicodeEncoding.BigEndianUnicode.GetString(currentMessage.ToArray());

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

			try
			{
                if(results.Count != 6 || results[3].Contains("upgrade") || results[1]=="8" || results[2].Contains("1.7"))
				{
                    //throw new Exception(SERVER_ERROR_NOT_SUPPORTED);
                    server.Protocol = 127;
					return false;
				}
				else
				{
					string players = results.ElementAt(4) + "/" + results.ElementAt(5);
					server.Info = players;
					server.MOTD = results.ElementAt(3);
					server.Protocol = Convert.ToInt32(results.ElementAt(1));
				}
			}

			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw new Exception(SERVER_ERROR_UNDERSTAND_SERVER);
				return false;
			}

			return true;

		}

		private bool ProtocolV2Ping(Server server)
		{
			try
			{
				//TcpClient client = null;
                Socket client = null;

				try
				{
					//client = GetTcpClient(server);
                    client = Global.GetSocket(server);
				}
				catch
				{
					throw new Exception(SERVER_ERROR_CANT_REACH);
				}

				//NetworkStream stream  = client.GetStream();
                NetworkStream stream = new NetworkStream(client);

				// This is the new protocol, need to ping differently
				List<byte> handshake = new List<byte>();
				handshake.Add(0x00);

                if(server.Protocol == 127)
                {
                    handshake.AddRange(mcStreamReader.WriteRawVarint32(Global.CurrentReleaseProtocol));
                }
                else
                {
                    handshake.AddRange(mcStreamReader.WriteRawVarint32(server.Protocol));
                }

                handshake.AddRange(mcStreamReader.WriteRawVarint32(server.Address.Length));
                handshake.AddRange(Encoding.ASCII.GetBytes(server.Address));

                handshake.AddRange(BitConverter.GetBytes(Convert.ToUInt16(server.RealPort)).Reverse());
				handshake.Add(0x01);
				handshake.InsertRange(0, mcStreamReader.WriteRawVarint32(handshake.Count));

                //Debug.WriteLine(BitConverter.ToString( handshake.ToArray() ));
				stream.Write(handshake.ToArray(), 0, handshake.Count);
                //client.Send(handshake.ToArray());

                handshake.Clear();
				handshake.AddRange(mcStreamReader.WriteRawVarint32(1));
				handshake.AddRange(mcStreamReader.WriteRawVarint32(0));

                //Debug.WriteLine(BitConverter.ToString( handshake.ToArray() ));
				stream.Write(handshake.ToArray(), 0, handshake.Count);
                //client.Send(handshake.ToArray());

				try
				{
                    int i = 0;
                    while(i == 0)
                    {
                        i = (int)mcStreamReader.ReadRawVarint32(stream);
                    }
				}
                catch(Exception y)
				{
                    //Debug.WriteLine(y.Message);
					// Must not be v2 protocol
                    throw new Exception(SERVER_ERROR_UNDERSTAND_SERVER);
                }

				mcStreamReader.ReadByte(stream);

				string handShakeResponse = mcStreamReader.ReadString(stream);
                //Console.WriteLine(handShakeResponse);
				try
				{
					//JsonValue hs = JsonObject.Parse(handShakeResponse);
                    JObject hs = JObject.Parse(handShakeResponse);

                    server.Protocol = (int)hs["version"]["protocol"];

                    try
                    {
                        string versionName = hs["version"]["name"].ToString();

                        if(versionName.Contains("1.8"))
                        {
                            server.Protocol = 47;
                        }
                    }
                    catch
                    {
                        //ignore
                    }

					string players = hs["players"]["online"].ToString() + "/" + hs["players"]["max"].ToString();
					server.Info = players;

                    try
                    {
                        //JsonValue description = hs["description"].Type == JTokenType.String;

                        if(hs["description"].Type == JTokenType.String)
                        {
                            server.MOTD = hs["description"].ToString();
                        }
                        else
                        {
                            server.MOTD = hs["text"].ToString();
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        string favIcon = hs["favicon"].ToString();
    				    string[] favSplit = favIcon.Split(",".ToCharArray());
    				    server.FavIcon = Convert.FromBase64String(favSplit[1]);
                    }
                    catch
                    {
                        server.FavIcon = null;
                    }
    					
                    try
                    {
                        server.players.Clear();
                        JToken onlinePlayers = hs["players"]["sample"];                       
                        foreach(JValue currentPlayer in onlinePlayers)
                        {                                                         
                            string cp = currentPlayer["name"].ToString();
                            server.players.Add(cp);
                        }
                    }
                    catch(Exception ex)
                    {
                        //Console.WriteLine("Error FavIcon: " + ex.Message);
                    }

                    //Console.WriteLine("Server Name: " + server.ServerName + " Protocol: " + server.Protocol.ToString());

				}
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
                    throw new Exception(SERVER_ERROR_UNDERSTAND_SERVER + " " + ex.Message);
				}

				return true; 
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
                throw ex;
			}
		}

        /*
        private Socket GetSocket(Server server)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult result = socket.BeginConnect(server.Address, Convert.ToInt16(server.Port), null, null); 

            bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            if (!success)
            {
                throw new Exception("Failed to connect.");
            }

            // we have connected
            return socket;
        }
        */

        /*
		private TcpClient GetTcpClient(Server server)
		{

			TcpClient client = new TcpClient();

            IAsyncResult result = client.BeginConnect(server.RealIP, server.RealPort, null, null);

			bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(7));

			if (!success)
			{
				throw new Exception("Failed to connect.");
			}

			// we have connected
            client.EndConnect(result);
			return client;

		}

        private bool GetRealIPAndPort(string host, int port, out IPAddress realIP, out int realPort)
        {
            //Console.WriteLine("GetRealIP: " + host);
            IPHostEntry ip;
            realIP = null;
            realPort = 0;

            try
            {
                if (!IPAddress.TryParse(host, out realIP))
                {
                    // Dealing with a host name
                    //Resolver resolver = new Resolver(Resolver.DefaultDnsServers);
                    string dnsServer = Platform.GetDNSServerAddress();

                    IPEndPoint[] ds = null;
                    if(dnsServer != string.Empty)
                    {
                        ds = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse(dnsServer), 53), new IPEndPoint(IPAddress.Parse("8.8.4.4"), 53)};
                    }
                    else
                    {
                        ds = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53), new IPEndPoint(IPAddress.Parse("8.8.4.4"), 53)};
                    }

                    Resolver resolver = new Resolver(ds);
                    resolver.TimeOut = 5;
                    resolver.Retries = 1;
                    resolver.UseCache = true;
                    resolver.TransportType = Heijden.DNS.TransportType.Udp;

                    //Resolver resolver = new Resolver(dnsServer);
                    Response response; 

                    response = resolver.Query("_minecraft._tcp." + host + ".", QType.SRV);


                    if (response.Answers.Count() == 0)
                    {
                        //Console.WriteLine("no service record");
                        // no service record, get the IP directly
                        ip = resolver.Resolve(host);
                        if(ip.AddressList.Count() == 0)
                        {
                            realIP = IPAddress.None;
                        }
                        else
                        {
                            realIP = ip.AddressList[0];
                        }
                        realPort = port;
                        //Console.WriteLine(host + " " + realIP + ":" + realPort.ToString());
                        return false;
                    }
                    else
                    {
                        //Console.WriteLine("got service record");
                        // Got an srv record so grab that as the real info
                        for (int currentRecord = 0; currentRecord < response.Answers.Count;)
                            //foreach(var answer in response.Answers)
                        {
                            AnswerRR rec = response.Answers[currentRecord];

                            if (rec.Type.ToString() == "SRV")
                            {

                                RecordSRV recordSRV = (RecordSRV)rec.RECORD;
                                ip = resolver.Resolve(recordSRV.TARGET);

                                if(port!=25565)
                                {
                                    realPort = port;
                                }
                                else
                                {
                                    realPort = recordSRV.PORT; 
                                }
                            }
                            else
                            {
                                RecordCNAME recordCNAME = (RecordCNAME)rec.RECORD;
                                ip = resolver.Resolve(recordCNAME.CNAME);
                                realPort = port;
                            }

                            if (ip.AddressList.Count() == 0)
                            {
                                throw new Exception("Could not resolve address");
                            }


                            realIP = ip.AddressList[0];

                            if(realIP.ToString() == "67.215.65.132")
                            {
                                // This is a googleDNS not found entry.  Try to use the host name
                                IPAddress temp;
                                string hostIP = ip.HostName;
                                if(hostIP.EndsWith("."))
                                {
                                    hostIP = hostIP.Substring(0, hostIP.Length - 1);
                                }

                                if(IPAddress.TryParse(hostIP, out temp))
                                {
                                    realIP = temp;
                                }
                                else
                                {
                                    throw new Exception("Could not resolve address");
                                }
                            }

                            currentRecord++;
                        }
                    }
                }
                else
                {
                    realPort = port;
                }

                //Console.WriteLine(host + " " + realIP + ":" + realPort.ToString());


                return true;

            }
            catch (Exception ex)
            {
                //Console.WriteLine("error resolving");
                throw new Exception(ex.Message);
            }
        }

        void HandleOnVerbose (object sender, Resolver.VerboseEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private int ReadRawVarint32(Stream stream, out int bytesRead)
        {
            bytesRead = 1;

            int num = 0;
            int num2 = 0;
            while (num2 != 35)
            {
                int b = stream.ReadByte();
                num |= (int)((int)(b & 127) << (num2 & 31));
                num2 += 7;
                if ((b & 128) == 0)
                {
                    return num;
                }
                bytesRead++;
            }

            throw new FormatException("Format_Bad7BitInt32");
        }
         */
    }
}

