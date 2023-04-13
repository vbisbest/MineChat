﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.IO.Compression;

namespace MineChatAPI
{
    public class PacketV5 : IPacket
    {
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

        public PacketV5(MineStream stream)
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
                    if(IsImportantPacket(packet))
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
                case PacketType.PluginMessage:
                    //ReadPluginMessage();
                    break;
                case PacketType.PlayerPositionAndLook:
                    //ProcessPlayerPositionAndLook();
                    break;
                case PacketType.UpdateHealth:
                    ProcessUpdateHealth();
                    break;
                case PacketType.PreLoginSuccess:
                    if (!GotLoginSuccess)
                    {
                        ReadLoginSuccess();
                    }
                    break;
                case PacketType.PreDisconnect:
                    if(!GotLoginSuccess)
                    {
                        ReadDisconnected();
                    }
                    break;
                case PacketType.KeepAlive:
                    this.ProcessKeepAlive();
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
                case PacketType.ChatMessage:
                    this.ReadChatMessage();
                    break;
                case PacketType.PreSetCompression:
                    if (!GotLoginSuccess)
                    {
                        ReadSetCompression();
                    }
                    break;                  
                case PacketType.PlayerListItem:
                    ReadPlayerListItem();
                    break;
                case PacketType.Disconnect:
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

            messageList.Add(0x03);
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

            if(UpdateHealth!=null)
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


        private void SendPluginMessage(string type, string message )
        {
            List<byte> clientSettings = new List<byte>();
            if (compressionSet)
            {
                clientSettings.Add(0x00); // Not Compressed
            }

            clientSettings.Add((int)PacketType.ClientPluginMessage); // Packet ID
            clientSettings.AddRange(stream.WriteRawVarint32(System.Text.Encoding.UTF8.GetBytes(type).Length)); 
            clientSettings.AddRange(System.Text.Encoding.UTF8.GetBytes(type));
            clientSettings.AddRange(stream.WriteRawVarint32(System.Text.Encoding.UTF8.GetBytes(message).Length));
            clientSettings.AddRange(System.Text.Encoding.UTF8.GetBytes(message));
            clientSettings.InsertRange(0, stream.WriteRawVarint32(clientSettings.Count));

            stream.Write(clientSettings.ToArray(), 0, clientSettings.Count);
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
                            packetReader.ReadString();
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
                        packetReader.ReadString();
                    }
                }
                else
                {
                    // Player went offline
                    PlayerOnline(playerName, false, uuid.ToString());
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

        public void SendSpawn()
        {
            SendChat("/spawn");
        }

        private void ReadLoginSuccess()
        {
            packetReader.ReadString();
            packetReader.ReadString();
            GotLoginSuccess = true;
            //SendClientSettings();
            //RequestStats();

            if (Connected != null)
            {
                Connected();
            }
        }

        public void ReadPluginMessage()
        {
            try
            {
                byte[] t;

                t = packetReader.ReadToEnd();

                packetReader.SetCurrentPacket(t);

                string x = packetReader.ReadString();
                string y = string.Empty;

                List<byte> list = new List<byte>();
                

                if (x == "REGISTER")
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
                else if (x == "MC|Brand")
                {
                    //t = packetReader.ReadToEnd();
                    //y = packetReader.ReadStringClassic();
                    y = packetReader.ReadString();
                    SendClientSettings();
                    SendPluginMessage(x, "vanilla");
                }
                else if (x == "VentureChat")
                {
                    //byte[] a = packetReader.ReadToEnd();
                    //string y = packetReader.ReadString();
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
                    //Debug.WriteLine("Plugin: " + x);
                }
                //byte[] b = packetReader.ReadByteArray(6);

                //string y = packetReader.ReadString();

                //Debug.WriteLine ("Plugin: " + x);

                //int l = packetReader.ReadInt();
                //short length = packetReader.ReadShort();
                //Console.WriteLine(length.ToString());
                //byte[] q = packetReader.ReadByteArray(l);


            }
            catch(Exception ex) {
                Debug.WriteLine (ex.Message);
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
            SpawnObject = 0x00,
            SpawnExperienceOrb = 0x01,
            SpawnGlobalEntity = 0x02,
            SpawnMob = 0x03,
            SpawnPainting = 0x04,
            SpawnPlayer = 0x05,
            Animation = 0x06,
            Statistics = 0x07,
            BlockBreakAnimation = 0x08,
            UpdateBlockEntity = 0x09,
            BlockAction = 0x0A,
            BlockChange = 0x0B,
            BossBar = 0x0C,
            ServerDifficulty = 0x0D,
            TabComplete = 0x0E,
            ChatMessage = 0x0F,
            MultiBlockChange = 0x10,
            ConfirmTransaction = 0x11,
            CloseWindow = 0x12,
            OpenWindow = 0x13,
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
            KeepAlive = 0x1F,
            ChunkData = 0x20,
            Effect = 0x21,
            Particle = 0x22,
            JoinGame = 0x23,
            Map = 0x24,
            EntityRelativeMove = 0x25,
            EntityLookAndRelativeMove = 0x26,
            EntityLook = 0x27,
            Entity = 0x28,
            VehicleMove = 0x29,
            OpenSignEditor = 0x2A,
            PlayerAbilities = 0x2B,
            CombatEvent = 0x2C,
            PlayerListItem = 0x2D,
            PlayerPositionAndLook = 0x2E,
            UseBed = 0x2F,
            DestroyEntities = 0x30,
            RemoveEntityEffect = 0x31,
            ResourcePackSend = 0x32,
            Respawn = 0x33,
            EntityHeadLook = 0x34,
            WorldBorder = 0x35,
            Camera = 0x36,
            HeldItemChange = 0x37,
            DisplayScoreboard = 0x38,
            EntityMetadata = 0x39,
            AttachEntity = 0x3A,
            EntityVelocity = 0x3B,
            EntityEquipment = 0x3C,
            SetExperience = 0x3D,
            UpdateHealth = 0x3E,
            ScoreboardObjective = 0x3F,
            SetPassengers = 0x40,
            Teams = 0x41,
            UpdateScore = 0x42,
            SpawnPosition = 0x43,
            TimeUpdate = 0x44,
            Title = 0x45,
            UpdateSign = 0x46,
            SoundEffect = 0x47,
            PlayerListHeaderAndFooter = 0x48,
            CollectItem = 0x49,
            EntityTeleport = 0x4A,
            EntityProperties = 0x4B,
            EntityEffect = 0x4C,
            ServerKeepAlive = 0x0B,
            ServerClientSettings = 0x04,
            ServerChatMessage = 0x02,
            ClientPluginMessage = 0x09

        }
    }
}
