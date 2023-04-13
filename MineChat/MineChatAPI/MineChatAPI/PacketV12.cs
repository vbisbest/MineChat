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
    public class PacketV12 : IPacket
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

        public PacketV12(MineStream stream)
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
            importantPackets.Add(PacketType.PlayerInfo);
            //importantPackets.Add(PacketType.PluginMessage);
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

            //if (packet != PacketType.MultiBlockChange && packet != PacketType.EntityHeadLook && packet != PacketType.PlayerPositionAndLook)
            //{
            //    string packetName = ((PacketType)packet).ToString();
            //    Console.WriteLine(packetName);
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
                    if (debug) Console.WriteLine("UpdateHealth");
                    ProcessUpdateHealth();
                    break;
                case PacketType.PreLoginSuccess:
                    if (!GotLoginSuccess)
                    {
                        if (debug) Console.WriteLine("PreLoginSuccess");
                        ReadLoginSuccess();
                    }
                    break;
                case PacketType.PreDisconnect:
                    if (!GotLoginSuccess)
                    {
                        if (debug) Console.WriteLine("PreDisconnect");
                        ReadDisconnected();
                    }
                    break;
                case PacketType.KeepAlive:
                    if (debug) Console.WriteLine("KeepAlive");
                    this.ProcessKeepAlive();
                    break;
                case PacketType.PreEncryptionRequest:
                    if (GotLoginSuccess)
                    {
                        if (debug) Console.WriteLine("LoginSuccess");
                        SendClientSettings();
                    }
                    else
                    {
                        if (debug) Console.WriteLine("PreEncryptionRequest");
                        ProcessEncryptionRequest();
                    }
                    break;
                case PacketType.ChatMessage:
                    if (debug) Console.WriteLine("ChatMessage");
                    this.ReadChatMessage();
                    break;
                case PacketType.PreSetCompression:
                    if (!GotLoginSuccess)
                    {
                        if (debug) Debug.WriteLine("PreSetCompression");
                        ReadSetCompression();
                    }
                    break;
                case PacketType.PlayerInfo:
                    if (debug) Debug.WriteLine("PlayerListItem");
                    ReadPlayerInfo();
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

        public void SendSpawn()
        {
            SendChat("/spawn");
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
            ChatType chatType = (ChatType)packetReader.ReadByte();            
            var senderId = packetReader.ReadUUID();

            if (ChatMessage != null)
            {
                ChatMessage(message, chatType);
            }
        }

        public void ReadPlayerInfo()
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
            var x = packetReader.ReadUUID();
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

        public void ProcessKeepAlive()
        {
            if (!GotLoginSuccess)
            {
                ReadDisconnected();
                return;
            }

            List<byte> keepAlive = new List<byte>();

            byte[] id = packetReader.ReadByteArray(8);
            
            if (compressionSet)
            {
                // add uncompressed packet
                keepAlive.Add(0x00);
            }

            keepAlive.AddRange(stream.WriteRawVarint32((int)PacketType.ServerKeepAlive));
            keepAlive.AddRange(id);
            keepAlive.InsertRange(0, stream.WriteRawVarint32(keepAlive.Count));

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

            SpawnLivingEntity = 0x02,
            SpawnPainting = 0x03,
            SpawnPlayer = 0x04,
            EntityAnimation = 0x05,
            Statistics = 0x06,
            AcknowledgePlayerDigging = 0x07,
            BlockBreakAnimation = 0x08,
            BlockEntityData = 0x09,
            BlockAction = 0x0A,
            BlockChange = 0x0B,
            BossBar = 0x0C,
            ServerDifficulty = 0x0D,
            ChatMessage = 0x0E,
            MultiBlockChange = 0x0F,
            TabComplete = 0x10,
            DeclareCommands = 0x11,
            WindowConfirmation = 0x12,
            CloseWindow = 0x13,
            WindowItems = 0x14,
            WindowProperty = 0x15,
            SetSlot = 0x16,
            SetCooldown = 0x17,
            PluginMessage = 0x18,
            NamedSoundEffect = 0x19,
            Disconnect = 0x1A,
            EntityStatus = 0x1B,
            Explosion = 0x1C,
            UnloadChunk = 0x1D,
            ChangeGameState = 0x1E,
            OpenHorseWindow = 0x1F,
            KeepAlive = 0x20,
            ChunkDataPre = 0x21,
            Effect = 0x22,
            Particle = 0x23,
            UpdateLightPre = 0x24,
            JoinGame = 0x25,
            MapData = 0x26,
            TradeList = 0x27,
            EntityPosition = 0x28,
            EntityPositionandRotation = 0x29,
            EntityRotation = 0x2A,
            EntityMovement = 0x2B,
            VehicleMove = 0x2C,
            OpenBook = 0x2D,
            OpenWindow = 0x2E,
            OpenSignEditor = 0x2F,
            CraftRecipeResponse = 0x30,
            PlayerAbilities = 0x31,
            CombatEvent = 0x32,
            PlayerInfo = 0x33,
            FacePlayer = 0x34,
            PlayerPositionAndLook = 0x35,
            UnlockRecipes = 0x36,
            DestroyEntities = 0x37,
            RemoveEntityEffect = 0x38,
            ResourcePackSend = 0x39,
            RespawnPre = 0x3A,
            EntityHeadLook = 0x3B,
            SelectAdvancementTab = 0x3C,
            WorldBorder = 0x3D,
            Camera = 0x3E,
            HeldItemChange = 0x3F,
            UpdateViewPosition = 0x40,
            UpdateViewDistance = 0x41,
            EntityEquipmentPre = 0x47,
            UpdateHealth = 0x49,
            SpawnPosition = 0x42,
            TimeUpdate = 0x4E,
            Title = 0x4F,            
            EntitySoundEffect = 0x50,
            SoundEffect = 0x51,
            StopSound = 0x52,
            PlayerListHeaderAndFooter = 0x53,
            NBTQueryResponse = 0x54,
            CollectItem = 0x55,
            EntityTeleport = 0x56,
            Advancements = 0x57,
            EntityProperties = 0x58,
            EntityEffect = 0x59,
            DeclareRecipesPre = 0x5A,
            Tags = 0x5B,

            ServerChatMessage = 0x03,
            ServerClientStatus = 0x04,
            ServerClientSettings = 0x05,
            ServerTabComplete = 0x06,
            ServerKeepAlive = 0x10,
            ClientPluginMessage = 0x0B
        }
    }
}
