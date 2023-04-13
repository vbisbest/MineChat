using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.IO.Compression;

namespace MineChatAPI
{
    public class PacketV10 : IPacket
    {
        private bool debug = false;

        private bool GotLoginSuccess;
        private int packetCount = 0;
        byte[] encryptedSharedSecret = null;
        byte[] encryptedVerification = null;
        byte[] serverSharedKey = null;


        private bool compressionSet = false;
        private PacketReader packetReader = new PacketReader(30000);
        MineStream stream = null;

        private string serverID = string.Empty;
        List<PacketType> importantPackets = new List<PacketType>();

        public event ChatMessageHandler ChatMessage;
        public event DisconnectedHandler Disconnected;
        public event PlayerOnlineHandler PlayerOnline;
        public event KeepAliveHandler KeepAlive;
        public event ConnectedHandler Connected;
        public event ConnectErrorHandler ConnectError;
        public event UpdateHealthHandler UpdateHealth;

        float yaw = 280;
        float pitch = 100;
        double x = 1000;
        double y = 1000;
        double z = 1000;

        private Account currentAccount = null;

        public PacketV10(MineStream stream)
        {
            this.stream = stream;
            currentAccount = Global.CurrentSettings.GetSelectedAccount();

            importantPackets.Add(PacketType.PreLoginSuccess);
            importantPackets.Add(PacketType.PreDisconnect);
            importantPackets.Add(PacketType.ChatMessage);
            importantPackets.Add(PacketType.KeepAlive);
            importantPackets.Add(PacketType.PreEncryptionRequest);
            importantPackets.Add(PacketType.PreSetCompression);
            importantPackets.Add(PacketType.Disconnect);
            importantPackets.Add(PacketType.PlayerListItem);
            importantPackets.Add(PacketType.PluginMessage);
            importantPackets.Add(PacketType.UpdateHealth);
            //importantPackets.Add(PacketType.PlayerPositionAndLook);

        }

        private int GetPacketLength()
        {
            return (int)stream.ReadRawVarint32();
        }

        private bool IsImportantPacket(PacketType packet)
        {
            return importantPackets.Contains(packet);
        }

        public void ProcessStream()
        {
            int len = 0;

            // Get the packet length
            while (len == 0)
            {
                len = GetPacketLength();
            }

            int bytesRead = 0;
            int compressedBytes = 0;
            int compressBytesRead = 0;
            int bytesToRead = 0;
            PacketType packet = 0;

            if (compressionSet)
            {
                compressedBytes = stream.ReadRawVarint32(out compressBytesRead);
                len = len - compressBytesRead;

                if (len == 0)
                {
                    // 0 byte packet
                    return;
                }

                if (compressedBytes > 3000)
                {
                    // Dont care about huge packets
                    stream.ReadByteArray(len, false);
                    return;
                }
                else if (compressedBytes > 0)
                {
                    try
                    {
                        byte[] b = stream.ReadByteArray(len, true);
                        DeflateStream zlibStream = new DeflateStream(new MemoryStream(b, 2, b.Length - 2), CompressionMode.Decompress);
                        packetReader.SetCurrentPacket(zlibStream);

                        // Get the packet type
                        packet = (PacketType)packetReader.ReadRawVarint32(out bytesRead);
                    }
                    catch (Exception ce)
                    {
                        throw ce;
                    }
                }
                else
                {
                    packet = (PacketType)stream.ReadRawVarint32(out bytesRead);

                    // How many bytes are in the packet
                    bytesToRead = (len - bytesRead);
                    if (IsImportantPacket(packet))
                    {
                        packetReader.SetCurrentPacket(stream.ReadByteArray(bytesToRead, true));
                    }
                    else
                    {
                        stream.ReadByteArray(bytesToRead, false);
                    }

                }
            }
            else
            {
                // Get the packet type
                packet = (PacketType)stream.ReadRawVarint32(out bytesRead);

                // How many bytes are in the packet
                bytesToRead = (len - bytesRead);

                // Grab the bytes
                packetReader.SetCurrentPacket(stream.ReadByteArray(bytesToRead, IsImportantPacket(packet)));
            }



            //if (packet != PacketType.MultiBlockChange && packet != PacketType.EntityHeadLook && packet != PacketType.EntityLookAndRelativeMove)
            //{
            // string packetName = ((PacketType)packet).ToString();
            // System.Diagnostics.Debug.WriteLine(packetName);
            //}


            switch (packet)
            {
                case PacketType.PluginMessage:
                    //ReadPluginMessage();
                    break;
                case PacketType.PlayerPositionAndLook:
                    //ProcessPlayerPositionAndLook();
                    break;
                case PacketType.UpdateHealth:
                    if (debug) Debug.WriteLine("UpdateHealth");
                    ProcessUpdateHealth();
                    break;
                case PacketType.PreLoginSuccess:
                    if (!GotLoginSuccess)
                    {
                        if (debug) Debug.WriteLine("PreLoginSuccess");
                        ReadLoginSuccess();
                    }
                    break;
                case PacketType.PreDisconnect:
                    if (!GotLoginSuccess)
                    {
                        if (debug) Debug.WriteLine("PreDisconnect");
                        ReadDisconnected();
                    }
                    break;
                case PacketType.KeepAlive:
                    if (debug) Debug.WriteLine("KeepAlive");
                    this.ProcessKeepAlive();
                    break;
                case PacketType.PreEncryptionRequest:
                    if (GotLoginSuccess)
                    {
                        if (debug) Debug.WriteLine("LoginSuccess");
                        SendClientSettings();
                    }
                    else
                    {
                        if (debug) Debug.WriteLine("PreEncryptionRequest");
                        ProcessEncryptionRequest();
                    }
                    break;
                case PacketType.ChatMessage:
                    if (debug) Debug.WriteLine("ChatMessage");
                    this.ReadChatMessage();
                    break;
                case PacketType.PreSetCompression:
                    if (!GotLoginSuccess)
                    {
                        if (debug) Debug.WriteLine("PreSetCompression");
                        ReadSetCompression();
                    }
                    break;
                case PacketType.PlayerListItem:
                    if (debug) Debug.WriteLine("PlayerListItem");
                    ReadPlayerListItem();
                    break;
                case PacketType.Disconnect:
                    if (debug) Debug.WriteLine("Disconnect");
                    ReadDisconnected();
                    break;
                default:
                    break;
            }
        }

        public void RequestStats()
        {
            List<byte> messageList = new List<byte>();

            if (compressionSet)
            {
                // No compression
                messageList.Add(0);
            }

            messageList.Add(0x03);
            messageList.AddRange(stream.WriteRawVarint32(0x01));
            messageList.InsertRange(0, stream.WriteRawVarint32(messageList.Count));

            stream.Write(messageList.ToArray(), 0, messageList.Count);
        }

        public void Respawn()
        {
            List<byte> messageList = new List<byte>();

            if (compressionSet)
            {
                // No compression
                messageList.Add(0);
            }

            messageList.Add((byte)PacketType.ServerClientStatus);
            messageList.AddRange(stream.WriteRawVarint32(0x00));
            messageList.InsertRange(0, stream.WriteRawVarint32(messageList.Count));

            stream.Write(messageList.ToArray(), 0, messageList.Count);
        }

        public void SendChat(string message)
        {
            try
            {
                List<byte> messageList = new List<byte>();

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                if (compressionSet)
                {
                    // No compression
                    messageList.Add(0);
                }

                messageList.AddRange(stream.WriteRawVarint32((int)PacketType.ServerChatMessage));


                messageList.AddRange(stream.WriteRawVarint32(messageBytes.Length));
                messageList.AddRange(messageBytes);
                messageList.InsertRange(0, stream.WriteRawVarint32(messageList.Count));

                stream.Write(messageList.ToArray(), 0, messageList.Count);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Send chat failure: " + ex.Message);
            }
        }

        private void ProcessPlayerPositionAndLook()
        {
            x = packetReader.ReadDouble();
            y = packetReader.ReadDouble();
            z = packetReader.ReadDouble();
            yaw = packetReader.ReadFloat();
            pitch = packetReader.ReadFloat();
            byte flags = packetReader.ReadByte();
            int teleportid = packetReader.ReadRawVarint32();

            List<byte> messageList = new List<byte>();

            if (compressionSet)
            {
                // No compression
                messageList.Add(0);
            }
            //Debug.WriteLine("asdfasdfasdfasdfasdf");
            messageList.Add(0x00);
            messageList.AddRange(packetReader.WriteRawVarint32(teleportid));
            messageList.InsertRange(0, stream.WriteRawVarint32(messageList.Count));
            stream.Write(messageList.ToArray(), 0, messageList.Count);

        }

        private void ProcessUpdateHealth()
        {
            float h = packetReader.ReadFloat();
            int f = packetReader.ReadRawVarint32();
            float fs = packetReader.ReadFloat();

            if (UpdateHealth != null)
            {
                UpdateHealth(h, f);
            }
        }

        private void ProcessEncryptionRequest()
        {
            serverID = packetReader.ReadString();

            int keyLen = 0;

            if (Global.CurrentProtocol > 19)
            {
                keyLen = packetReader.ReadRawVarint32();
            }
            else
            {
                keyLen = packetReader.ReadShort();
            }

            byte[] publicKey = packetReader.ReadByteArray(keyLen);

            if (Global.CurrentProtocol > 19)
            {
                keyLen = packetReader.ReadRawVarint32();
            }
            else
            {
                keyLen = packetReader.ReadShort();
            }

            byte[] verifyToken = packetReader.ReadByteArray(keyLen);
            InitializeEncryption(publicKey, verifyToken);
        }

        private void InitializeEncryption(byte[] publicKey, byte[] verifyToken)
        {
            Debug.WriteLine("Initialize Encryption");

            try
            {
                // Generate our shared secret
                serverSharedKey = PKCS1Signature.CreateSecretKey();

                var pkcs = new PKCS1Signature(publicKey);
                encryptedSharedSecret = pkcs.SignData(serverSharedKey);
                encryptedVerification = pkcs.SignData(verifyToken);

                DoSessionLogin(serverSharedKey, publicKey);
            }
            catch (Exception cryptError)
            {
                if (ConnectError != null)
                {
                    ConnectError("Error initializing encryption: " + cryptError.Message);
                }
            }
        }

        private void DoSessionLogin(byte[] serverSharedKey, byte[] publicKey)
        {
            Debug.WriteLine("DoSessionLogin");

            // Generate session hash
            byte[] hashData = Encoding.UTF8.GetBytes(serverID)
                .Concat(serverSharedKey)
                    .Concat(publicKey).ToArray();

            var hash = PKCS1Signature.GetServerHash(serverID, publicKey, serverSharedKey);
            string sessionId = string.Format("token:{0}:{1}", currentAccount.AccessToken, currentAccount.ProfileID);

            MojangAPI mojangAPI = new MojangAPI();
            mojangAPI.CreateSessionComplete += mojangAPI_CreateSessionComplete;
            //mojangAPI.CreateSession(currentAccount.PlayerName, sessionId, hash);
            mojangAPI.CreateSession(currentAccount.AccessToken, currentAccount.ProfileID, hash);
        }

        public void SendClientSettings()
        {
            List<byte> clientSettings = new List<byte>();
            if (compressionSet)
            {
                clientSettings.Add(0x00); // Not Compressed
            }

            clientSettings.Add((int)PacketType.ServerClientSettings); // Packet ID
            clientSettings.AddRange(stream.WriteRawVarint32(System.Text.Encoding.UTF8.GetBytes("en_us").Length)); // Local
            clientSettings.AddRange(System.Text.Encoding.UTF8.GetBytes("en_us")); // Local
            clientSettings.Add(0x03); // Render dist
            clientSettings.Add(0x00); // Chat mode
            clientSettings.Add(0x01); // chat colors
            clientSettings.Add(0x7f); // Display skin
            clientSettings.Add(0x01); // Main hand
            clientSettings.InsertRange(0, stream.WriteRawVarint32(clientSettings.Count));

            stream.Write(clientSettings.ToArray(), 0, clientSettings.Count);
        }


        private void SendPluginMessage(string type, string message)
        {
            List<byte> pluginMessage = new List<byte>();
            if (compressionSet)
            {
                pluginMessage.Add(0x00); // Not Compressed
            }

            pluginMessage.Add((int)PacketType.ClientPluginMessage); // Packet ID
            pluginMessage.AddRange(stream.WriteRawVarint32(System.Text.Encoding.UTF8.GetBytes(type).Length));
            pluginMessage.AddRange(System.Text.Encoding.UTF8.GetBytes(type));
            pluginMessage.AddRange(stream.WriteRawVarint32(System.Text.Encoding.UTF8.GetBytes(message).Length));
            pluginMessage.AddRange(System.Text.Encoding.UTF8.GetBytes(message));
            pluginMessage.InsertRange(0, stream.WriteRawVarint32(pluginMessage.Count));

            stream.Write(pluginMessage.ToArray(), 0, pluginMessage.Count);
        }

        void mojangAPI_CreateSessionComplete(AuthResponse authResponse)
        {
            if (authResponse.Result == RequestResult.Success)
            {
                CompleteInitialize();
            }
            else
            {
                if (ConnectError != null)
                {
                    ConnectError("Error creating session: " + authResponse.Message);
                }
            }
        }

        private void CompleteInitialize()
        {
            try
            {
                List<byte> establishedResponse = new List<byte>();

                establishedResponse.AddRange(stream.WriteRawVarint32(encryptedSharedSecret.Length));
                establishedResponse.AddRange(encryptedSharedSecret);

                establishedResponse.AddRange(stream.WriteRawVarint32(encryptedVerification.Length));
                establishedResponse.AddRange(encryptedVerification);

                establishedResponse.Insert(0, (int)PacketType.PreEncryptionResponse);
                establishedResponse.InsertRange(0, stream.WriteRawVarint32(establishedResponse.Count));

                Debug.WriteLine("Send encyption response");
                stream.Write(establishedResponse.ToArray(), 0, establishedResponse.Count);

                stream.EnableEncryption(serverSharedKey);
            }
            catch (Exception ex)
            {
                if (ConnectError != null)
                {
                    ConnectError("Error completing initialize: " + ex.Message);
                }
            }
        }

        public void ReadChatMessage()
        {
            string message = packetReader.ReadString();

            //Debug.WriteLine(message);

            ChatType chatType = (ChatType)packetReader.ReadByte();

            if (ChatMessage != null)
            {
                ChatMessage(message, chatType);
            }
        }

        public void ReadPlayerListItem()
        {
            string playerName = string.Empty;

            int action = packetReader.ReadRawVarint32();
            int count = packetReader.ReadRawVarint32();

            for (int i = 0; i < count; i++)
            {
                byte[] guidBytes = packetReader.ReadByteArray(16);
                string uuid = BitConverter.ToString(guidBytes).ToLower();
                uuid = uuid.Replace("-", "");

                if (action == 0)
                {
                    playerName = packetReader.ReadString();

                    int propCount = packetReader.ReadRawVarint32();

                    for (int j = 0; j < propCount; j++)
                    {
                        packetReader.ReadString();
                        packetReader.ReadString();

                        bool isSigned = packetReader.ReadBool();
                        if (isSigned)
                        {
                            packetReader.ReadString();
                        }
                    }

                    packetReader.ReadRawVarint32();
                    packetReader.ReadRawVarint32();

                    if (Global.CurrentProtocol > 27)
                    {
                        bool b = packetReader.ReadBool();
                        if (b)
                        {
                            //Console.WriteLine("do something");

                            string message = packetReader.ReadString();

                            /*
                            Debug.WriteLine(message);

                            if (ChatMessage != null)
                            {
                                ChatMessage(message);
                            }
                            */
                        }
                    }

                    if (!playerName.ToLower().Contains("?") && !playerName.Contains(" ") && !playerName.Contains("("))
                    {
                        PlayerOnline(playerName, true, uuid);
                    }

                }
                else if (action == 1)
                {
                    packetReader.ReadRawVarint32();
                }
                else if (action == 2)
                {
                    packetReader.ReadRawVarint32();
                }
                else if (action == 3)
                {
                    bool b = packetReader.ReadBool();
                    if (b)
                    {
                        string message = packetReader.ReadString();
                        /*
                        Debug.WriteLine(message);
                            
                        if (ChatMessage != null)
                        {
                            ChatMessage(message);
                        }
                        */
                    }
                }
                else
                {
                    // Player went offline
                    PlayerOnline(playerName, false, uuid);
                }

            }
        }

        private void ReadSetCompression()
        {
            int x = packetReader.ReadRawVarint32();
            if (x > -1)
            {
                compressionSet = true;
            }
        }

        private void ReadDisconnected()
        {
            string reason = packetReader.ReadString();
            if (this.Disconnected == null)
                return;
            this.Disconnected(reason);
        }

        private void ReadLoginSuccess()
        {
            string x = packetReader.ReadString();
            string y = packetReader.ReadString();

            GotLoginSuccess = true;

            if (Connected != null)
            {
                Connected();
            }
        }

        public void ReadPluginMessage()
        {
            try
            {
                byte[] bytes;

                bytes = packetReader.ReadToEnd();
                packetReader.SetCurrentPacket(bytes);

                string message = packetReader.ReadString();

                if (message == "REGISTER")
                {
                    /*
                    t = packetReader.ReadToEnd();
                    //Debug.WriteLine("register");
                    string ty = Encoding.UTF8.GetString(t, 0, t.Length);

                    for (int i = 0; i < t.Length; i++)
                    {
                        if (t[i] == '\0')
                        {
                            y = Encoding.UTF8.GetString(list.ToArray(), 0, list.Count());
                            Debug.WriteLine("Register: " + y);
                            list.Clear();
                        }
                        else
                        {
                            list.Add(t[i]);
                        }
                    }  
                    */
                }
                else if (message == "MC|Brand")
                {
                    //t = packetReader.ReadToEnd();
                    //y = packetReader.ReadStringClassic();
                    packetReader.ReadString();
                    SendPluginMessage(message, "MineChat");
                }
                else if (message == "VentureChat")
                {
                    //byte[] a = packetReader.ReadToEnd();
                    string g = packetReader.ReadString();
                    //t = packetReader.ReadToEnd();
                    //string xx = Encoding.UTF8.GetString(t, 0, t.Length);
                    //y = packetReader.ReadString();
                    //y = packetReader.ReadString();
                    //y = packetReader.ReadString();
                    //y = packetReader.ReadString();
                    //y = packetReader.ReadString();
                    //y = packetReader.ReadString();

                    /*
                    for (int i = 0; i < t.Length; i++)
                    {
                        if (t[i] == '\0')
                        {
                            y = Encoding.UTF8.GetString(list.ToArray(), 0, list.Count());
                            Debug.WriteLine("VentureChat: " + y);
                            list.Clear();
                        }
                        else
                        {
                            list.Add(t[i]);
                        }
                    }
                    */
                }
                else
                {
                    Debug.WriteLine("Plugin: " + x);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public void SendSpawn()
        {
            SendChat("/spawn");
        }

        public void ProcessKeepAlive()
        {
            if (!GotLoginSuccess)
            {
                ReadDisconnected();
                return;
            }

            List<byte> keepAlive = new List<byte>();



            if (Global.CurrentProtocol > 338)
            {
                //long lid = 0;
                //lid = packetReader.ReadLong();
                byte[] lid = packetReader.ReadByteArray(8);

                if (compressionSet)
                {
                    // add uncompressed packet
                    keepAlive.Add(0x00);
                }

                keepAlive.AddRange(stream.WriteRawVarint32((int)PacketType.ServerKeepAlive));
                keepAlive.AddRange(lid);
                keepAlive.InsertRange(0, stream.WriteRawVarint32(keepAlive.Count));
            }
            else
            {
                int id = 0;
                id = packetReader.ReadRawVarint32();

                if (compressionSet)
                {
                    // add uncompressed packet
                    keepAlive.Add(0x00);
                }

                keepAlive.AddRange(stream.WriteRawVarint32((int)PacketType.ServerKeepAlive));
                keepAlive.AddRange(stream.WriteRawVarint32(id));
                keepAlive.InsertRange(0, stream.WriteRawVarint32(keepAlive.Count));
            }

            stream.Write(keepAlive.ToArray(), 0, keepAlive.Count);

            if (KeepAlive != null)
            {
                KeepAlive();
            }
        }

        private enum PacketType
        {
            PreDisconnect = 0x00,
            PreEncryptionRequest = 0x01,
            PreLoginSuccess = 0x02,
            PreSetCompression = 0x03,
            PreEncryptionResponse = 0x01,

            SpawnObject = 0x00,
            SpawnExperienceOrb = 0x01,
            SpawnGlobalEntity = 0x02,
            SpawnMob = 0x03,
            SpawnPainting = 0x04,
            SpawnPlayer = 0x05,
            Animation = 0x06,
            Statistics = 0x07,
            PlayerDigging = 0x08,
            BlockBreakAnimation = 0x09,
            UpdateBlockEntity = 0x0A,
            BlockAction = 0x0B,
            BlockChange = 0x0C,
            BossBar = 0x0D,
            ServerDifficulty = 0x0E,
            ChatMessage = 0x0F,
            MultiBlockChange = 0x10,
            TabComplete = 0x11,
            DeclareCommands = 0x12,
            ConfirmTransaction = 0x13,
            CloseWindow = 0x14,            
            WindowItems = 0x15,
            WindowProperty = 0x16,
            SetSlot = 0x17,
            SetCooldown = 0x18,
            PluginMessage = 0x19,
            NamedSoundEffect = 0x1A,
            Disconnect = 0x1B,
            EntityStatus = 0x1C,            
            Explosion = 0x1D,
            UnloadChunk = 0x1E,
            ChangeGameState = 0x1F,
            OpenHorseWindows = 0x20,
            KeepAlive = 0x21,
            ChunkData = 0x22,
            Effect = 0x23,
            Particle = 0x24,
            UpdateLight = 0x25,
            JoinGame = 0x26,
            MapData = 0x27,
            TradeList = 0x28,            
            EntityRelativeMove = 0x29,
            EntityLookAndRelativeMove = 0x2A,
            EntityLook = 0x2B,
            Entity = 0x2C,
            VehicleMove = 0x2D,
            OpenBook = 0x2E,
            OpenWindow = 0x2F,
            OpenSignEditor = 0x30,
            CraftRecipieResponse = 0x31,
            PlayerAbilities = 0x32,
            CombatEvent = 0x33,
            PlayerListItem = 0x34,
            FacePlayer = 0x35,
            PlayerPositionAndLook = 0x36,            
            UnlockRecipes = 0X37,
            DestroyEntities = 0x38,
            RemoveEntityEffect = 0x39,
            ResourcePackSend = 0x3A,
            Respawn = 0x3B,
            EntityHeadLook = 0x3C,
            SelectAdvancementTab = 0x3D,
            WorldBorder = 0x3E,
            Camera = 0x3F,
            HeldItemChange = 0x40,
            UpdateViewPosition = 0x41,
            UpdateViewDistance = 0x42,
            DisplayScoreboard = 0x43,
            EntityMetadata = 0x44,
            AttachEntity = 0x45,
            EntityVelocity = 0x46,
            EntityEquipment = 0x47,
            SetExperience = 0x48,
            UpdateHealth = 0x49,
            ScoreboardObjective = 0x4A,
            SetPassengers = 0x4B,
            Teams = 0x4C,
            UpdateScore = 0x4D,
            SpawnPosition = 0x4E,
            TimeUpdate = 0x4F,
            Title = 0x50,
            EntitySoundEffect = 0x51,
            SoundEffect = 0x52,
            StopSound = 0x53,
            PlayerListHeaderAndFooter = 0x54,
            NBTQueryResponse = 0x55,
            CollectItem = 0x56,
            EntityTeleport = 0x57,
            Advancements = 0x58,
            EntityProperties = 0x59,
            EntityEffect = 0x5A,
            DeclareRecipes = 0x5B,
            Tags = 0x5C,

            ServerChatMessage = 0x03,
            ServerClientStatus = 0x04,
            ServerClientSettings = 0x05,
            ServerTabComplete = 0x06,       
            ServerKeepAlive = 0x0F,
            ClientPluginMessage = 0x0B
        }
    }
}
