using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace MineChatAPI
{
    public enum ServerStatus
    {
        Unknown,
        Polling,
        Error,
        OK
    }

    public enum ProtocolVersion
    {
        V3,
        V4
    }

    public enum ServerTypeEnum
    {
        Featured,
        Promoted,
        User,
        Realms
    }

    [XmlRootAttribute("Server")]
    public class Server
    {
        public Server()
        {
            this.FullAddress = string.Empty;
            this.Info = string.Empty;
            this.MOTD = string.Empty;
            this.ServerType = ServerTypeEnum.User;
            this.RealIP = string.Empty;
        }

        private string serverName = string.Empty;
        private string motd = string.Empty;
        private string address = string.Empty;        
        private int serverVersion = 0;
        private string id = string.Empty;

        public Server Clone()
        {
            Server newServer;

            try
            {
                newServer = new Server();
                newServer.FullAddress = this.FullAddress;
                newServer.FavIcon = this.FavIcon;
                newServer.Info = this.Info;
                newServer.LastInfoUpdate = this.LastInfoUpdate;
                newServer.MOTD = this.MOTD;
                newServer.PromotedServerID = this.PromotedServerID;
                newServer.Protocol = this.Protocol;
                newServer.ProtocolVersion = this.ProtocolVersion;
                newServer.RealIP = this.RealIP;
                newServer.RealPort = this.RealPort;
                newServer.ServerName = this.ServerName;
                newServer.ServerType = this.ServerType;
                newServer.ServerVersion = this.ServerVersion;
                newServer.Status = this.Status;
                newServer.UseOffline = this.UseOffline;
                newServer.ID = this.ID;
            }
            catch(Exception ex)
            {
                throw new Exception("Could not clone server: " + ex.Message);
            }

            return newServer;

        }

        public string ServerName
        {
            get
            {
                return serverName;
            }
            set
            {
                serverName = value;
            }
        }

        public string MOTD
        {
            get
            {
                return motd;
            }
            set
            {
                motd = value;
            }
        }


        public string FullAddress
        {
            get
            {                
                if (this.Port == "25565")
                {
                    return this.Address.Trim();
                }
                else
                {
                    return this.Address + ":" + this.Port.ToString();
                }
            }
            set
            {
                try
                {   
                    string temp = value.Trim();

                    if (temp.Contains(":"))
                    {
                        string[] values = temp.Split(":".ToCharArray());
                        address = values[0];
                        this.Port = values[1];
                    }
                    else
                    {
                        address = temp;
                        this.Port = "25565";
                    }
                }
                catch
                {
                    Debug.WriteLine("Error setting server address");
                }
            }
        }


        public string Port { get; set; }
        public bool UseOffline { get; set; }
        public int Protocol { get; set; }
        public int RealPort { get; set; }
        public byte[] FavIcon { get; set; }
        public string RealIP { get; set; }

        [XmlIgnore]
        public Server State = null;


        [XmlIgnore]
        public ProtocolVersion ProtocolVersion = ProtocolVersion.V4;

        [XmlIgnore]
        public List<string> Players = new List<string>();

        [XmlIgnore]
        public DateTime LastInfoUpdate;

        public string Info { get; set; }

        [XmlIgnore]
        public ServerStatus Status = ServerStatus.Unknown;

        public override string ToString()
        {
            return this.ServerName;
        }

        public string ID
        {
            get
            {
                string r = string.Empty;

                //return this.ServerType.ToString() + this.ServerName + this.Address;
                if(id == string.Empty)
                {
                    r =  Guid.NewGuid().ToString();
                }
                else
                {
                    r = id;
                }

                return r;
            }
            set
            {
                id = value;
            }
        }

        public int ServerVersion
        {
            get
            {
                return serverVersion;
            }
            set
            {
                serverVersion = value;
            }
        }

        public string Address
        {
            get
            {
                return address.Trim();
            }
            //set
            //{
            //    address = value;
            //}
        }


        private string GetAddress()
        {
            return string.Empty;
        }

        public ServerTypeEnum ServerType { get; set; }
        public int PromotedServerID { get; set; }
    }
}

