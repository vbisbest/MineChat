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
    public class PacketV4 : IPacket
    {
        private bool GotLoginSuccess;

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

        private Account currentAccount = null; 

        public PacketV4(MineStream stream)
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
            importantPackets.Add(PacketType.SetCompression);
            importantPackets.Add(PacketType.UpdateHealth);

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

            //string packetName = ((PacketType)packet).ToString();
            //System.Diagnostics.Debug.WriteLine(packetName);

            switch (packet)
            {
                case PacketType.UpdateHealth:
                    ProcessUpdateHealth();
                    break;
                case PacketType.PreLoginSuccess:
                    if (!GotLoginSuccess)
                    {
                        ReadLoginSuccess();
                    }
                    else
                    {
                        this.ReadChatMessage();
                    }
                    break;
                case PacketType.PreDisconnect:
                    if (!GotLoginSuccess)
                    {
                        ReadDisconnected();
                    }
                    else
                    {
                        // Keep alive
                        this.ProcessKeepAlive();
                    }
                    break;
                case PacketType.PreEncryptionRequest:
                    if (GotLoginSuccess)
                    {
                        SendClientSettings();
                    }
                    else
                    {
                        ProcessEncryptionRequest();
                    }
                    break;
                case PacketType.PreSetCompression:
                    if (!GotLoginSuccess)
                    {
                        ReadSetCompression();
                    }
                    break;
                case PacketType.SetCompression:
                    ReadSetCompression();
                    break;
                case PacketType.PlayerListItem:
                    if (Global.CurrentProtocol > 19)
                    {
                        ReadPlayerListItem2();
                    }
                    else
                    {
                        ReadPlayerListItem();
                    }
                    break;
                case PacketType.Disconnect:
                    ReadDisconnected();
                    break;
                default:
                    break;
            }
        }

        public void Respawn()
        {
            List<byte> messageList = new List<byte>();

            if (compressionSet)
            {
                // No compression
                messageList.Add(0);
            }

            //messageList.Add(0x16);
            messageList.Add(0x03);
            //messageList.AddRange(stream.WriteRawVarint32(0x01));
            messageList.Add(0x01);

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
                messageList.AddRange(stream.WriteRawVarint32(0x01));


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

        private void ReadLoginSuccess()
        {
            packetReader.ReadString();
            packetReader.ReadString();
            GotLoginSuccess = true;

            if (Connected != null)
            {
                Connected();
            }
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
            try
            {
                Debug.WriteLine("DoSessionLogin");

                var hash = PKCS1Signature.GetServerHash(serverID, publicKey, serverSharedKey);
                string sessionId = string.Format("token:{0}:{1}", currentAccount.AccessToken, currentAccount.ProfileID);

                MojangAPI mojangAPI = new MojangAPI();
                mojangAPI.CreateSessionComplete += mojangAPI_CreateSessionComplete;
                //mojangAPI.CreateSession(currentAccount.PlayerName, sessionId, hash);
                mojangAPI.CreateSession(currentAccount.AccessToken, currentAccount.ProfileID, hash);
            }
            catch(Exception ex)
            {
                if (ConnectError != null)
                {
                    ConnectError("Unable to create session. Minecraft servers may be down");
                }
            }
        }

        private void SendClientSettings()
        {
            if (Global.CurrentProtocol > 50)
            {
                SendClientSettings19();
                return;
            }

            try
            {
                List<byte> messageList = new List<byte>();
                byte[] temp = null;

                if (compressionSet)
                {
                    Debug.WriteLine("Send Client Settings++");
                    temp = new byte[] { 0x0C, 0x00, 0x15, 0x05, 0x65, 0x6E, 0x5F, 0x55, 0x53, 0x0C, 0x00, 0x01, 0x01 };
                }
                else if (Global.CurrentProtocol > 12)
                {
                    Debug.WriteLine("Send Client Settings+");
                    temp = new byte[] { 0x0B, 0x15, 0x05, 0x65, 0x6E, 0x5F, 0x55, 0x53, 0x0C, 0x00, 0x01, 0x01 };

                }
                else
                {
                    Debug.WriteLine("Send Client Settings");
                    temp = new byte[] { 0x0C, 0x15, 0x05, 0x65, 0x6E, 0x5F, 0x55, 0x53, 0x0C, 0x00, 0x01, 0x02, 0x01 };
                }

                messageList.AddRange(temp);
                stream.Write(messageList.ToArray(), 0, messageList.Count);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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

                if (Global.CurrentProtocol > 19)
                {
                    establishedResponse.AddRange(stream.WriteRawVarint32(encryptedSharedSecret.Length));
                }
                else
                {
                    establishedResponse.AddRange(BitConverter.GetBytes((short)encryptedSharedSecret.Length).Reverse());
                }
                establishedResponse.AddRange(encryptedSharedSecret);

                if (Global.CurrentProtocol > 19)
                {
                    establishedResponse.AddRange(stream.WriteRawVarint32(encryptedVerification.Length));
                }
                else
                {
                    establishedResponse.AddRange(BitConverter.GetBytes((short)encryptedVerification.Length).Reverse());
                }

                establishedResponse.AddRange(encryptedVerification);

                establishedResponse.Insert(0, 0x01);
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

        private void SendClientSettings19()
        {
            List<byte> messageList = new List<byte>();
            byte[] temp = null;

            if (compressionSet)
            {
                Debug.WriteLine("Send Client Settings19++");
                temp = new byte[] { 0x0D, 0x00, 0x16, 0x05, 0x65, 0x6E, 0x5F, 0x55, 0x53, 0x03, 0x00, 0x01, 0x01, 0x01 };
            }
            else
            {
                Debug.WriteLine("Send Client Settings19+");
                temp = new byte[] { 0x0c, 0x16, 0x05, 0x65, 0x6E, 0x5F, 0x55, 0x53, 0x03, 0x00, 0x01, 0x01, 0x01 };

            }

            messageList.AddRange(temp);
            stream.Write(messageList.ToArray(), 0, messageList.Count);
        }

        private void ReadChatMessage()
        {
            ChatType chatType = ChatType.ChatBox;

            if (!this.GotLoginSuccess)
            {
                packetReader.ReadString();
                packetReader.ReadString();
                this.GotLoginSuccess = true;
            }
            else
            {
                string message = packetReader.ReadString();
                if (Global.CurrentProtocol > 12)
                {
                    chatType = (ChatType)packetReader.ReadByte();
                }
                if (this.ChatMessage == null)
                    return;

                this.ChatMessage(message, chatType);
            }
        }

        private void ReadPlayerListItem()
        {
            string name = packetReader.ReadString();
            bool online = packetReader.ReadBool();
            if (Global.CurrentProtocol > 12)
            {
                packetReader.ReadRawVarint32();
            }
            else
            {
                int num = (int)packetReader.ReadShort();
            }
            if (this.PlayerOnline == null)
                return;

            this.PlayerOnline(name, online, string.Empty);
        }


        private void ReadPlayerListItem2()
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
                            packetReader.ReadString();
                        }
                    }

                    if (PlayerOnline != null && playerName != string.Empty)
                    {
                        PlayerOnline(playerName, true, uuid.ToString());
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
                        packetReader.ReadString();
                    }
                }
                else
                {
                    // Player went offline
                    if (PlayerOnline != null)
                    {
                        PlayerOnline(playerName, false, uuid.ToString());
                    }
                }
            }
        }

        public void SendSpawn()
        {
            SendChat("/spawn");
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

        private void ProcessKeepAlive()
        {
            if (!GotLoginSuccess)
            {
                ReadDisconnected();
                return;
            }

            List<byte> keepAlive = new List<byte>();
            int id = 0;

            if (Global.CurrentProtocol > 27)
            {
                id = packetReader.ReadRawVarint32();
            }
            else
            {
                id = packetReader.ReadInt();
            }

            keepAlive.Add(0x00);

            if (compressionSet)
            {
                // add uncompressed packet
                keepAlive.Add(0x00);
            }

            if (Global.CurrentProtocol > 12)
            {
                keepAlive.AddRange(stream.WriteRawVarint32(id));
            }
            else
            {
                byte[] returnID = BitConverter.GetBytes(id);
                keepAlive.AddRange(returnID);
            }

            keepAlive.InsertRange(0, stream.WriteRawVarint32(keepAlive.Count));

            stream.Write(keepAlive.ToArray(), 0, keepAlive.Count);

            if (KeepAlive != null)
            {
                KeepAlive();
            }

            /*
            List<byte> messageList = new List<byte>();

            if (compressionSet)
            {
                // No compression
                messageList.Add(0);
            }

            messageList.Add(0x03);
            messageList.Add(0x01);

            messageList.InsertRange(0, stream.WriteRawVarint32(messageList.Count));

            stream.Write(messageList.ToArray(), 0, messageList.Count);
            */
        }

        private enum PacketType
        {
            PreLoginSuccess = 0x02,
            PreSetCompression = 0x03,
            PreDisconnect = 0x00,
            PreEncryptionRequest = 0x01,
            SpawnObject = 0x0E,
            SpawnExperienceOrb = 0x11,
            SpawnGlobalEntity = 0x2C,
            SpawnMob = 0x0F,
            SpawnPainting = 0x10,
            SpawnPlayer = 0x0C,
            Animation = 0x0B,
            Statistics = 0x37,
            BlockBreakAnimation = 0x25,
            UpdateBlockEntity = 0x35,
            BlockAction = 0x24,
            BlockChange = 0x23,
            ServerDifficulty = 0x41,
            TabComplete = 0x3A,
            ChatMessage = 0x02,
            MultiBlockChange = 0x22,
            ConfirmTransaction = 0x32,
            CloseWindow = 0x2E,
            OpenWindow = 0x2D,
            WindowItems = 0x30,
            WindowProperty = 0x31,
            SetSlot = 0x2F,
            PluginMessage = 0x3F,
            Disconnect = 0x40,
            Explosion = 0x27,
            SetCompression = 0x46,
            ChangeGameState = 0x2B,
            KeepAlive = 0x00,
            ChunkData = 0x21,
            Effect = 0x28,
            Particle = 0x2A,
            SoundEffect = 0x29,
            JoinGame = 0x01,
            Map = 0x34,
            EntityRelativeMove = 0x15,
            EntityLookAndRelativeMove = 0x17,
            EntityLook = 0x16,
            Entity = 0x14,
            OpenSignEditor = 0x36,
            PlayerAbilities = 0x39,
            CombatEvent = 0x42,
            PlayerListItem = 0x38,
            PlayerPositionAndLook = 0x08,
            UseBed = 0x0A,
            DestroyEntities = 0x13,
            RemoveEntityEffect = 0x1E,
            ResourcePackSend = 0x48,
            Respawn = 0x07,
            EntityHeadLook = 0x19,
            WorldBorder = 0x44,
            Camera = 0x43,
            HeldItemChange = 0x09,
            DisplayScoreboard = 0x3D,
            EntityMetadata = 0x1C,
            AttachEntity = 0x1B,
            EntityVelocity = 0x12,
            EntityEquipment = 0x04,
            SetExperience = 0x1F,
            UpdateHealth = 0x06,
            ScoreboardObjective = 0x3B,
            Teams = 0x3E,
            UpdateScore = 0x3C,
            SpawnPosition = 0x05,
            TimeUpdate = 0x03,
            Title = 0x45,
            UpdateSign = 0x33,
            PlayerListHeaderAndFooter = 0x47,
            CollectItem = 0x0D,
            EntityTeleport = 0x18,
            EntityProperties = 0x20,
            EntityEffect = 0x1D,
            MapChunkBulk = 0x26,
            UpdateEntityNBT = 0x49
        }
    }
}
