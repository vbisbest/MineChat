using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace MineChatAPI
{
    public class PacketV3 : IPacket
    {

        public event ChatMessageHandler ChatMessage;
        public event DisconnectedHandler Disconnected;
        public event PlayerOnlineHandler PlayerOnline;
        public event KeepAliveHandler KeepAlive;
        public event ConnectedHandler Connected;
        public event ConnectErrorHandler ConnectError;
        public event UpdateHealthHandler UpdateHealth;

        private string serverID = string.Empty;
        MineStream stream = null;
        byte[] encryptedSharedSecret = null;
        byte[] encryptedVerification = null;
        byte[] serverSharedKey = null;

        const int DATA_TYPE_BYTE = 0;
        const int DATA_TYPE_SHORT = 1;
        const int DATA_TYPE_INT = 2;
        const int DATA_TYPE_LONG = 3;
        const int DATA_TYPE_FLOAT = 4;
        const int DATA_TYPE_DOUBLE = 5;
        const int DATA_TYPE_STRING = 6;
        const int DATA_TYPE_BOOL = 7;
        const int DATA_TYPE_METADATA = 8;
        const int DATA_TYPE_SLOT = 9;
        const int DATA_TYPE_BYTEARRAY = 10;
        const int DATA_TYPE_ARRAYOFINT = 11;
        const int DATA_TYPE_EXPLOSION = 12;
        const int DATA_TYPE_METAINFO = 13;
        const int DATA_TYPE_UNSIGNEDSHORT = 14;
        const int DATA_TYPE_UNSIGNEDBYTEARRAY = 15;
        const int DATA_TYPE_UNSIGNEDBYTE = 16;
        const int DATA_TYPE_OBJECTDATA = 17;
        Dictionary<byte, List<byte>> packetDefinitions;

        Account currentAccount = null;


        public PacketV3(MineStream stream)
        {
            this.stream = stream;

            currentAccount = Global.CurrentSettings.GetSelectedAccount();

            // Build the definitions
            packetDefinitions = new Dictionary<byte,List<byte>>();
            packetDefinitions.Add(Convert.ToByte(PacketType.KeepAlive), new List<byte>() { DATA_TYPE_BYTE });                      
            packetDefinitions.Add(Convert.ToByte(PacketType.LoginRequest), new List<byte>() { DATA_TYPE_INT, DATA_TYPE_STRING, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BYTE });                      
            packetDefinitions.Add(Convert.ToByte(PacketType.Handshake), new List<byte>() { DATA_TYPE_BYTE, DATA_TYPE_STRING, DATA_TYPE_STRING, DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.ChatMessage), new List<byte>() {DATA_TYPE_STRING});
            packetDefinitions.Add(Convert.ToByte(PacketType.TimeUpdate), new List<byte>() {DATA_TYPE_LONG, DATA_TYPE_LONG});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityEquipment), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_SHORT, DATA_TYPE_SLOT});
            packetDefinitions.Add(Convert.ToByte(PacketType.SpawnPosition), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.UseEntity), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.UpdateHealth), new List<byte>() {DATA_TYPE_SHORT, DATA_TYPE_SHORT, DATA_TYPE_FLOAT});
            packetDefinitions.Add(Convert.ToByte(PacketType.Respawn), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_BYTE,DATA_TYPE_SHORT,DATA_TYPE_STRING});
            packetDefinitions.Add(Convert.ToByte(PacketType.Player), new List<byte>() {DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.PlayerPosition), new List<byte>() {DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.PlayerLook), new List<byte>() {DATA_TYPE_FLOAT,DATA_TYPE_FLOAT, DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.PlayerPositionAndLook), new List<byte>() {DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_FLOAT,DATA_TYPE_FLOAT,DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.PlayerDigging), new List<byte>() {DATA_TYPE_BYTE,DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_INT,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.PlayerBlockPlacement), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_UNSIGNEDBYTE, DATA_TYPE_INT, DATA_TYPE_BYTE ,DATA_TYPE_SLOT,DATA_TYPE_BYTE,DATA_TYPE_BYTE,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.HeldItemChange), new List<byte>() {DATA_TYPE_SHORT});
            packetDefinitions.Add(Convert.ToByte(PacketType.UseBed), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.Animation), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityAction), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.SpawnNamedEntity), new List<byte>() { DATA_TYPE_INT, DATA_TYPE_STRING, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_SHORT, DATA_TYPE_METADATA });
            packetDefinitions.Add(Convert.ToByte(PacketType.CollectItem), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.SpawnObjectOrVehicle), new List<byte>() { DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_OBJECTDATA});
            packetDefinitions.Add(Convert.ToByte(PacketType.SpawnMob), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_BYTE,DATA_TYPE_BYTE,DATA_TYPE_SHORT,DATA_TYPE_SHORT,DATA_TYPE_SHORT,DATA_TYPE_METADATA});
            packetDefinitions.Add(Convert.ToByte(PacketType.SpawnPainting), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_STRING, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.SpawnExperienceOrb), new List<byte>() {DATA_TYPE_INT ,DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_SHORT});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityVelocity), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_SHORT,DATA_TYPE_SHORT,DATA_TYPE_SHORT});
            packetDefinitions.Add(Convert.ToByte(PacketType.DestroyEntity), new List<byte>() {DATA_TYPE_ARRAYOFINT});
            packetDefinitions.Add(Convert.ToByte(PacketType.Entity), new List<byte>() {DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityRelativeMove), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_BYTE,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityLook), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityLookAndRelativeMove), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityTeleport), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityHeadLook), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityStatus), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.AttachEntity), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityMetadata), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_METADATA});
            packetDefinitions.Add(Convert.ToByte(PacketType.EntityEffect), new List<byte>() { DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_SHORT });            
            packetDefinitions.Add(Convert.ToByte(PacketType.RemoveEntityEffect), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.SetExperience), new List<byte>() {DATA_TYPE_FLOAT, DATA_TYPE_SHORT, DATA_TYPE_SHORT});
            packetDefinitions.Add(Convert.ToByte(PacketType.ChunkData), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_BOOL, DATA_TYPE_UNSIGNEDSHORT, DATA_TYPE_UNSIGNEDSHORT,DATA_TYPE_INT, DATA_TYPE_UNSIGNEDBYTEARRAY});
            packetDefinitions.Add(Convert.ToByte(PacketType.MultipleBlockChange), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_SHORT,DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.BlockChange), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_INT,DATA_TYPE_SHORT,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.BlockAction), new List<byte>() {DATA_TYPE_INT, DATA_TYPE_SHORT, DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_SHORT});
            packetDefinitions.Add(Convert.ToByte(PacketType.BlockBreakAnimation), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.MapChunkBulk), new List<byte>() {DATA_TYPE_SHORT,DATA_TYPE_INT,DATA_TYPE_BOOL,DATA_TYPE_BYTEARRAY, DATA_TYPE_METAINFO});
            packetDefinitions.Add(Convert.ToByte(PacketType.Explosion), new List<byte>() {DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_DOUBLE,DATA_TYPE_FLOAT,DATA_TYPE_INT, DATA_TYPE_EXPLOSION, DATA_TYPE_FLOAT,DATA_TYPE_FLOAT,DATA_TYPE_FLOAT});
            packetDefinitions.Add(Convert.ToByte(PacketType.SoundOrParticleEffect), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_INT,DATA_TYPE_INT, DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.NamedSoundEffect), new List<byte>() { DATA_TYPE_STRING, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_FLOAT, DATA_TYPE_BYTE });
            packetDefinitions.Add(Convert.ToByte(PacketType.ChangeGameState), new List<byte>() { DATA_TYPE_BYTE, DATA_TYPE_BYTE });
            packetDefinitions.Add(Convert.ToByte(PacketType.SpawnGlobalEntity), new List<byte>() { DATA_TYPE_INT, DATA_TYPE_BYTE, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT });
            packetDefinitions.Add(Convert.ToByte(PacketType.OpenWindow), new List<byte>() { DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_STRING, DATA_TYPE_BYTE });
            packetDefinitions.Add(Convert.ToByte(PacketType.CloseWindow), new List<byte>() { DATA_TYPE_BYTE });
            packetDefinitions.Add(Convert.ToByte(PacketType.ClickWindow), new List<byte>() { DATA_TYPE_BYTE, DATA_TYPE_SHORT, DATA_TYPE_BYTE, DATA_TYPE_SHORT, DATA_TYPE_BOOL, DATA_TYPE_SLOT });
            packetDefinitions.Add(Convert.ToByte(PacketType.SetWindowItems), new List<byte>() {DATA_TYPE_BYTE, DATA_TYPE_SHORT, DATA_TYPE_SLOT});                        
            packetDefinitions.Add(Convert.ToByte(PacketType.SetSlot), new List<byte>() {DATA_TYPE_BYTE,DATA_TYPE_SHORT,DATA_TYPE_SLOT});
            packetDefinitions.Add(Convert.ToByte(PacketType.UpdateWindowProperty), new List<byte>() {DATA_TYPE_BYTE,DATA_TYPE_SHORT,DATA_TYPE_SHORT});
            packetDefinitions.Add(Convert.ToByte(PacketType.ConfirmTransaction), new List<byte>() {DATA_TYPE_BYTE,DATA_TYPE_SHORT,DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.CreativeInventoryAction), new List<byte>() {DATA_TYPE_SHORT,DATA_TYPE_SLOT});
            packetDefinitions.Add(Convert.ToByte(PacketType.EnchantItem), new List<byte>() {DATA_TYPE_BYTE,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.UpdateSign), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_SHORT,DATA_TYPE_INT,DATA_TYPE_STRING,DATA_TYPE_STRING,DATA_TYPE_STRING,DATA_TYPE_STRING});
            packetDefinitions.Add(Convert.ToByte(PacketType.ItemData), new List<byte>() {DATA_TYPE_SHORT,DATA_TYPE_SHORT,DATA_TYPE_SHORT, DATA_TYPE_BYTEARRAY});
            packetDefinitions.Add(Convert.ToByte(PacketType.UpdateTileEntity), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_SHORT,DATA_TYPE_INT,DATA_TYPE_BYTE,DATA_TYPE_SHORT, DATA_TYPE_BYTEARRAY});
            packetDefinitions.Add(Convert.ToByte(PacketType.IncrementStatistic), new List<byte>() {DATA_TYPE_INT,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.PlayerListItem), new List<byte>() {DATA_TYPE_STRING,DATA_TYPE_BOOL,DATA_TYPE_SHORT});
            packetDefinitions.Add(Convert.ToByte(PacketType.PlayerAbilities), new List<byte>() {DATA_TYPE_BYTE,DATA_TYPE_BYTE,DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.TabComplete), new List<byte>() {DATA_TYPE_STRING});
            packetDefinitions.Add(Convert.ToByte(PacketType.ClientSettings), new List<byte>() {DATA_TYPE_STRING, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BYTE, DATA_TYPE_BOOL});
            packetDefinitions.Add(Convert.ToByte(PacketType.ClientStatus), new List<byte>() {DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.PluginMessage), new List<byte>() {DATA_TYPE_STRING, DATA_TYPE_SHORT, DATA_TYPE_BYTEARRAY});
            packetDefinitions.Add(Convert.ToByte(PacketType.EncryptionKeyResponse), new List<byte>() {DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.EncryptionKeyRequest), new List<byte>() {DATA_TYPE_STRING, DATA_TYPE_SHORT, DATA_TYPE_BYTEARRAY, DATA_TYPE_SHORT, DATA_TYPE_BYTEARRAY});
            packetDefinitions.Add(Convert.ToByte(PacketType.ServerListPing), new List<byte>() {DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.Disconnect), new List<byte>() {DATA_TYPE_STRING});
            packetDefinitions.Add(Convert.ToByte(PacketType.SteerVehicle), new List<byte>() {DATA_TYPE_FLOAT, DATA_TYPE_FLOAT, DATA_TYPE_BOOL, DATA_TYPE_BOOL});

            // protocol 59
            packetDefinitions.Add(Convert.ToByte(PacketType.CreateScoreboard), new List<byte>() {DATA_TYPE_STRING, DATA_TYPE_STRING, DATA_TYPE_BYTE});
            packetDefinitions.Add(Convert.ToByte(PacketType.UpdateScore), new List<byte>() {DATA_TYPE_STRING, DATA_TYPE_BYTE, DATA_TYPE_STRING, DATA_TYPE_INT});
            packetDefinitions.Add(Convert.ToByte(PacketType.DisplayScoreboard), new List<byte>() {DATA_TYPE_BYTE, DATA_TYPE_STRING});
            packetDefinitions.Add(Convert.ToByte(PacketType.Teams), new List<byte>() {DATA_TYPE_STRING, DATA_TYPE_BYTE, DATA_TYPE_STRING, DATA_TYPE_STRING, DATA_TYPE_STRING, DATA_TYPE_BYTE, DATA_TYPE_SHORT});

            packetDefinitions.Add(Convert.ToByte(PacketType.EntityProperties), new List<byte>() {DATA_TYPE_BYTEARRAY});
            packetDefinitions.Add(Convert.ToByte(PacketType.Particle), new List<byte>() {DATA_TYPE_STRING});
            packetDefinitions.Add(Convert.ToByte(PacketType.Unknown), new List<byte>() {DATA_TYPE_BYTE, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT, DATA_TYPE_INT});
        }

        public enum PacketType
        {
            KeepAlive =  0x00,
            LoginRequest =  0x01,
            Handshake =  0x02,
            ChatMessage =  0x03,
            TimeUpdate =  0x04,
            EntityEquipment =  0x05,
            SpawnPosition =  0x06,
            UseEntity =  0x07,
            UpdateHealth =  0x08,
            Respawn =  0x09,
            Player =  0x0A,
            PlayerPosition =  0x0B,
            PlayerLook =  0x0C,
            PlayerPositionAndLook =  0x0D,
            PlayerDigging =  0x0E,
            PlayerBlockPlacement =  0x0F,
            HeldItemChange =  0x10,
            UseBed =  0x11,
            Animation =  0x12,
            EntityAction =  0x13,
            SpawnNamedEntity =  0x14,
            CollectItem =  0x16,
            SpawnObjectOrVehicle =  0x17,
            SpawnMob =  0x18,
            SpawnPainting =  0x19,
            SpawnExperienceOrb =  0x1A,
            SteerVehicle = 0x1B,
            EntityVelocity =  0x1C,
            DestroyEntity =  0x1D,
            Entity =  0x1E,
            EntityRelativeMove =  0x1F,
            EntityLook =  0x20,
            EntityLookAndRelativeMove =  0x21,
            EntityTeleport =  0x22,
            EntityHeadLook =  0x23,
            EntityStatus =  0x26,
            AttachEntity =  0x27,
            EntityMetadata =  0x28,
            EntityEffect =  0x29,
            RemoveEntityEffect =  0x2A,
            SetExperience =  0x2B,
            EntityProperties = 0x2C,
            ChunkData =  0x33,
            MultipleBlockChange =  0x34,
            BlockChange =  0x35,
            BlockAction =  0x36,
            BlockBreakAnimation =  0x37,
            MapChunkBulk =  0x38,
            Explosion =  0x3C,
            SoundOrParticleEffect =  0x3D,
            NamedSoundEffect =  0x3E,
            Particle = 0x3F,
            ChangeGameState =  0x46,
            SpawnGlobalEntity =  0x47,
            OpenWindow =  0x64,
            CloseWindow =  0x65,
            ClickWindow =  0x66,
            SetSlot =  0x67,
            SetWindowItems =  0x68,
            UpdateWindowProperty =  0x69,
            ConfirmTransaction =  0x6A,
            CreativeInventoryAction =  0x6B,
            EnchantItem =  0x6C,
            UpdateSign =  0x82,
            ItemData =  0x83,
            UpdateTileEntity =  0x84,
            Unknown =  0x85,
            IncrementStatistic =  0xC8,
            PlayerListItem =  0xC9,
            PlayerAbilities =  0xCA,
            TabComplete =  0xCB,
            ClientSettings =  0xCC,
            ClientStatus =  0xCD,
            CreateScoreboard = 0xCE,
            UpdateScore = 0xCF,
            DisplayScoreboard = 0xD0,
            Teams = 0xD1, 
            PluginMessage =  0xFA,
            EncryptionKeyResponse =  0xFC,
            EncryptionKeyRequest =  0xFD,
            ServerListPing =  0xFE,
            Disconnect =  0xFF
        };

        public void ProcessStream()
        {
            // Read the first byte, should be a packet id
            PacketType packetType = (PacketType)ReadByte(stream);

            if (packetDefinitions.ContainsKey((byte)packetType))
            {
                //if(packetType!=PacketType.EntityHeadLook && packetType!=PacketType.EntityTeleport)
                //{
                // Debug.WriteLine(packetType.ToString());
                //}
                //Console.WriteLine (Global.CurrentProtocol.ToString());

                switch (packetType)
                {
                    case PacketType.ChangeGameState:
                        ReadChangeGameState(stream);
                        break;

                    case PacketType.SpawnNamedEntity:
                        ReadSpawnNamedEntity(stream);
                        break;

                    case PacketType.OpenWindow:
                        ReadOpenWindow(stream);
                        break;

                    case PacketType.LoginRequest:
                        ReadLoginRequest(stream);
                        break;

                    case PacketType.IncrementStatistic:
                        ReadIncrementStatistic (stream);
                        break;
                    case PacketType.Particle:
                        ReadParticle(stream);
                        break;
                    case PacketType.EntityProperties:
                        ReadEntityProperties(stream);
                        break;
                    case PacketType.PlayerAbilities:
                        ReadPlayerAbilities(stream);
                        break;
                    case PacketType.AttachEntity:
                        ReadAttachEntity(stream);
                        break;
                    case PacketType.UpdateHealth:
                        ReadUpdateHealth(stream);
                        break;
                    case PacketType.EntityAction:
                        ReadEntityAction(stream);
                        break;
                    case PacketType.PlayerPositionAndLook:
                        ReadPlayerPositionAndLook(stream);
                        break;
                    case PacketType.BlockAction:
                        ReadBlockAction(stream);
                        break;
                    case PacketType.SoundOrParticleEffect:
                        ReadSoundOrParticleEffect(stream);
                        break;
                    case PacketType.EntityLook:
                        ReadEntityLook(stream);
                        break;
                    case PacketType.EntityLookAndRelativeMove:
                        ReadEntityLookAndRelativeMove(stream);
                        break;
                    case PacketType.EntityRelativeMove:
                        ReadEntityRelativeMove(stream);
                        break;
                    case PacketType.EntityHeadLook:
                        ReadEntityHeadLook(stream);
                        break;
                    case PacketType.EntityTeleport:
                        ReadEntityTeleport(stream);
                        break;
                    case PacketType.Explosion:
                        ReadExplosion(stream);
                        break;
                    case PacketType.Teams:
                        ReadTeams(stream);
                        break;
                    case PacketType.UpdateScore:
                        ReadUpdateScore(stream);
                        break;
                    case PacketType.PlayerListItem:
                        ReadPlayerListItem(stream);
                        break;
                    case PacketType.EntityVelocity:
                        ReadEntityVelocity(stream);
                        break;
                    case PacketType.UpdateSign:
                        ReadUpdateSign(stream);
                        break;
                    case PacketType.MapChunkBulk:
                        ReadMapChunkBulk(stream);
                        break;
                    case PacketType.KeepAlive:
                        ProcessKeepAlive(stream);
                        break;
                    case PacketType.PluginMessage:
                        ReadPluginMessage(stream);
                        break;
                    case PacketType.SetWindowItems:
                        ReadSetWindowItems(stream);
                        break;
                    case PacketType.UpdateTileEntity:
                        ReadUpdateTileEntity(stream);
                        break;
                    case PacketType.ItemData:
                        ReadItemData(stream);
                        break;
                    case PacketType.SpawnObjectOrVehicle:
                        ReadSpawnObjectOrVehicle(stream);
                        break;
                    case PacketType.MultipleBlockChange:
                        ReadMultipleBlockChange(stream);
                        break;
                    case PacketType.Disconnect:
                        ReadDisconnected(stream);
                        break;
                    case PacketType.ChunkData:
                        ReadChunkData(stream);
                        break;
                    case PacketType.NamedSoundEffect:
                        ReadNamedSoundEffect(stream);
                        break;
                    case PacketType.SpawnMob:
                        ReadSpawnMob(stream);
                        break;
                    case PacketType.Player:
                        ReadPlayer(stream);
                        break;
                    case PacketType.ChatMessage:
                        string message = ReadString(stream, true);
                        if (ChatMessage != null)
                        {
                            ChatMessage(message, 0);
                        }
                        break;
                    case PacketType.EncryptionKeyRequest:
                        ProcessEncryptionRequest();
                        break;
                    case PacketType.EncryptionKeyResponse:
                        ProcessEncryptionResponse();
                        break;
                    default:
                        ProcessPacket(stream, packetDefinitions[(byte)packetType]);
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Missing packet definition " + packetType.ToString("X"));
            }

        }

        private void ProcessPacket(MineStream stream, List<byte> definition)
        {
            foreach(int currentDataType in definition)
            {
                switch (currentDataType)
                {
                    case DATA_TYPE_BYTE:
                        ReadUnsignedByte(stream);
                        break;
                    case DATA_TYPE_SHORT:
                        ReadShort(stream);
                        break;
                    case DATA_TYPE_INT:
                        ReadInt(stream);
                        break;
                    case DATA_TYPE_LONG:
                        ReadLong(stream);
                        break;
                    case DATA_TYPE_FLOAT:
                        ReadFloat(stream);
                        break;
                    case DATA_TYPE_DOUBLE:
                        ReadDouble(stream);
                        break;
                    case DATA_TYPE_STRING:
                        ReadString(stream);
                        break;
                    case DATA_TYPE_BOOL:
                        ReadBool(stream);
                        break;
                    case DATA_TYPE_METADATA:
                        ReadMetaData(stream);
                        break;
                    case DATA_TYPE_UNSIGNEDBYTE:
                        ReadByte(stream);
                        break;
                    case DATA_TYPE_UNSIGNEDBYTEARRAY:
                        ReadUnsignedByteArray(stream);
                        break;
                    case DATA_TYPE_UNSIGNEDSHORT:
                        ReadUnsingedShort(stream);
                        break;
                    case DATA_TYPE_EXPLOSION:
                        ReadExplosion(stream);
                        break;
                    case DATA_TYPE_METAINFO:
                        ReadMetaInfo(stream);
                        break;
                    case DATA_TYPE_SLOT:
                        ReadSlot(stream);
                        break;
                    case DATA_TYPE_ARRAYOFINT:
                        ReadArrayOfInt(stream);
                        break;
                    default:
                        //System.Diagnostics.Debug.Assert(false);
                        break;

                }
            }
        }

        public void Respawn()
        {
            List<byte> messageList = new List<byte>();

            messageList.Add(0x03);
            messageList.AddRange(stream.WriteRawVarint32(0x00));
            messageList.InsertRange(0, stream.WriteRawVarint32(messageList.Count));

            stream.Write(messageList.ToArray(), 0, messageList.Count);
        }

        private byte ReadByte(MineStream stream)
        {
            int buffer = stream.ReadByte();
            if (buffer == -1)
            {
                //Console.WriteLine("Issue");
            }
            return Convert.ToByte(buffer);
        }

        private void ProcessEncryptionRequest()
        {
            serverID = ReadString(stream, true);

            int keyLen = 0;

            keyLen = stream.ReadShort();
            byte[] publicKey = stream.ReadByteArray(keyLen);
            keyLen = stream.ReadShort();
            byte[] verifyToken = stream.ReadByteArray(keyLen);

            InitializeEncryption(publicKey, verifyToken);
     
        }

        private void ProcessEncryptionResponse()
        {
            int response = stream.ReadInt();

            stream.EnableEncryption(serverSharedKey);

            List<byte> ok = new List<byte>();
            ok.Add(Convert.ToByte(0xCD));
            ok.Add(0x00);
            stream.Write(ok.ToArray(), 0, ok.Count);



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

            var hash = PKCS1Signature.GetServerHash(serverID, publicKey, serverSharedKey);
            string sessionId = string.Format("token:{0}:{1}", currentAccount.AccessToken, currentAccount.ProfileID);

            MojangAPI mojangAPI = new MojangAPI();
            mojangAPI.CreateSessionComplete += mojangAPI_CreateSessionComplete;
            //mojangAPI.CreateSession(currentAccount.PlayerName, sessionId, hash);
            mojangAPI.CreateSession(currentAccount.AccessToken, currentAccount.ProfileID, hash);
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

                establishedResponse.AddRange(BitConverter.GetBytes((short)encryptedSharedSecret.Length).Reverse());
                establishedResponse.AddRange(encryptedSharedSecret);
                establishedResponse.AddRange(BitConverter.GetBytes((short)encryptedVerification.Length).Reverse());
                establishedResponse.AddRange(encryptedVerification);

                establishedResponse.Insert(0, 0xFC);

                Debug.WriteLine("Send encyption response");
                stream.Write(establishedResponse.ToArray(), 0, establishedResponse.Count);
            }
            catch (Exception ex)
            {
                if (ConnectError != null)
                {
                    ConnectError("Error completing initialize: " + ex.Message);
                }
            }
        }

        private short ReadShort(MineStream stream)
        {
            byte[] buffer = stream.Read(0, 2);
            Array.Reverse(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public void SendChat(string message)
        {
            try
            {
                List<byte> messageList = new List<byte>();
                messageList.Add(0x03);
                messageList.AddRange(BitConverter.GetBytes((short)message.Length).Reverse());
                messageList.AddRange(UnicodeEncoding.BigEndianUnicode.GetBytes(message));
                stream.Write(messageList.ToArray(), 0, messageList.Count);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Send chat failure: " + ex.Message);
            }
        }

        private string ReadString(MineStream stream, bool decrypt)
        {
            int size = (int)ReadShort(stream);  

            if (size == 0)
            {
                return string.Empty;
            }

            // Unicode
            size = size * 2;

            byte[] buffer = null;

            if(!decrypt && size > 16)
            {
                buffer = stream.Read(0, size, false);
            }
            else
            {
                buffer = stream.Read(0, size);
            }

            if(decrypt)
            {
                string s = Encoding.BigEndianUnicode.GetString(buffer, 0, size);
                return s;
            }
            else
            {
                return string.Empty;
            }

        }

        private string ReadString(MineStream stream)
        {
            return ReadString(stream, false);
        }

        private int ReadInt(MineStream stream)
        {
            byte[] buffer = stream.Read(0, 4);
            Array.Reverse(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        private uint ReadUnsignedInt(MineStream stream)
        {
            byte[] buffer = stream.Read(0, 4);
            Array.Reverse(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public long ReadLong(MineStream stream)
        {
            byte[] buffer = stream.Read(0, 8);
            Array.Reverse(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public sbyte ReadUnsignedByte(MineStream stream)
        {
            sbyte buffer = (sbyte)stream.ReadByte();
            return buffer;
        }

        public long ReadUnsignedByteArray(MineStream stream)
        {
            //System.Diagnostics.Debug.Assert(false);
            return 0;
        }

        public ushort ReadUnsingedShort(MineStream stream)
        {
            byte[] buffer = stream.Read(0, 2);
            Array.Reverse(buffer);
            return BitConverter.ToUInt16(buffer, 0);            
        }

        public long ReadMetaInfo(MineStream stream)
        {
            //System.Diagnostics.Debug.Assert(false);
            return 0;
        }
        public void ReadExplosion(MineStream stream)
        {
            ReadByteArray(stream, 28, false);
            int i = ReadInt(stream);
            int t = 3 * i;
            if(t > 0)
            {
                ReadByteArray(stream, t, false);
            }

            ReadByteArray(stream, 12, false);            
        }

        public void ReadArrayOfInt(MineStream stream)
        {
            var count = ReadByte(stream);

            for (int i = 0; i < count; i++)
            {
                ReadInt(stream);
            }
        }

        public byte[] ReadByteArray(MineStream stream, int length, bool decrypt)
        {
            byte[] buffer = null;

            if (length == 0)
                return buffer;

            if(length < 17)
            {
                decrypt = true;
            }

            buffer = stream.Read(0, length, decrypt);

            return buffer;

        }

        public byte[] ReadByteArray(MineStream stream, int length)
        {
            return ReadByteArray(stream, length, true);
        }

        public void ReadSlot(MineStream stream)
        {
            try
            {
                short id = ReadShort(stream);

                if (id == -1)
                    return;

                ReadByte(stream);
                ReadShort(stream);
                short length = ReadShort(stream);

                if (length == -1)
                    return;

                ReadByteArray(stream, length);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Issue: " + ex.Message);
            }
        }

        public void ReadMetaData(MineStream stream)
        {
            try
            {
                //sbyte currentByte = ReadUnsignedByte(stream);
                byte currentByte = ReadByte(stream);

                while (currentByte != 127)
                {
                    var index = currentByte & 0x1F; //# Lower 5 bits
                    var ty = currentByte >> 5; //  # Upper 3 bits
                    switch (ty)
                    {
                        case 0:
                            ReadByte(stream);
                            break;
                        case 1:
                            ReadShort(stream);
                            break;
                        case 2:
                            ReadInt(stream);
                            break;
                        case 3:
                            ReadFloat(stream);
                            break;
                        case 4:
                            ReadString(stream);
                            break;
                        case 5:
                            ReadSlot(stream);
                            break;
                        case 6:
                            ReadInt(stream);
                            ReadInt(stream);
                            ReadInt(stream);
                            break;

                    }
                    currentByte = ReadByte(stream);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }

        public bool ReadBool(MineStream stream)
        {
            byte x = ReadByte(stream);
            return x == Convert.ToByte(1);

        }

        public void ReadUpdateSign(MineStream stream)
        {
            ReadByteArray(stream, 10);
            ReadString(stream, false);
            ReadString(stream, false);
            ReadString(stream, false);
            ReadString(stream, false);
        }

        public void ReadMapChunkBulk(MineStream stream)
        {
            short chunkCount = ReadShort(stream);
            int dataLength = ReadInt(stream);
            ReadBool(stream);
            int totalBytes = (int)dataLength + ((int)chunkCount * 12);
            ReadByteArray(stream, totalBytes, false);
        }

        public void ReadItemData(MineStream stream)
        {
            ReadShort(stream);
            ReadShort(stream);
            short length = ReadShort(stream);

            int read = Convert.ToInt32(length);
            ReadByteArray(stream, read, false);
        }

        public void ReadEntityVelocity(MineStream stream)
        {
            ReadByteArray(stream, 10, true);
        }

        public void ReadSpawnObjectOrVehicle(MineStream stream)
        {
            try
            {
                ReadByteArray(stream, 19);

                int id = ReadInt(stream);

                if (id != 0)
                {
                    ReadByteArray(stream, 6);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }

        public void ReadUpdateTileEntity(MineStream stream)
        {
            ReadInt(stream);
            ReadShort(stream);
            ReadInt(stream);
            ReadByte(stream);
            short length = ReadShort(stream);

            if (length > 0)
                ReadByteArray(stream, (int)length, false);

        }

        public void ReadNamedSoundEffect(MineStream stream)
        {
            string s = ReadString (stream, true);

            if (Global.CurrentProtocol > 78) 
            {
                //Console.WriteLine("Sound: " + s);
                ReadByteArray (stream, 18, false);
                //byte[] b = ReadByteArray(stream, 100, true);

            } 
            else 
            {
                ReadByteArray(stream, 17, false);
            }
        }

        public void ReadTeams(MineStream stream)
        {
            string s;

            s = ReadString(stream, true);
            byte mode = ReadByte(stream);

            string sMode = Convert.ToString(mode);
            //Console.WriteLine(sMode);

            if(sMode == "0" || sMode == "2")
            {
                s = ReadString(stream, true);
                s = ReadString(stream, true);
                s = ReadString(stream, true);
                ReadByte(stream);
            }

            if(sMode == "0" || sMode == "3" || sMode == "4")
            {
                int players = ReadShort(stream);
                for(int i=0; i < players; i++)
                {
                    s = ReadString(stream, false);
                }
            }
        }

        public void ReadSoundOrParticleEffect(MineStream stream)
        {
            ReadByteArray(stream, 18, false);
        }

        public void SendSpawn()
        {
            SendChat("/spawn");
        }

        public void ReadLoginRequest(MineStream stream)
        {
            //byte[] x = ReadByteArray (stream, 150);

            ReadInt(stream);
            ReadString(stream, true);
            ReadByte(stream);

            int d = ReadInt(stream);

            if (Connected != null)
            {
                Connected();
            }

            // this tests for forge, if it is forge then get out
            if(d > 3)
            {
                return;
            }

            ReadByte(stream);
            ReadByte(stream);
            ReadByte(stream);



        }

        public void ReadEntityTeleport(MineStream stream)
        {
            ReadByteArray(stream, 18, false);
        }

        public void ReadEntityHeadLook(MineStream stream)
        {
            ReadByteArray(stream, 5);
        }

        public void ReadEntityRelativeMove(MineStream stream)
        {
            ReadByteArray(stream, 7);
        }

        public void ReadEntityLookAndRelativeMove(MineStream stream)
        {
            ReadByteArray(stream, 9);
        }

        private double ReadDoubleList(List<byte> l)
        {
            byte[] buffer = l.ToArray();
            Array.Reverse(buffer);
            double final = BitConverter.ToDouble(buffer, 0);
            return final;
        }

        public void ReadEntityAction(MineStream stream)
        {
            if(Global.CurrentProtocol > 62)
            {
                ReadByteArray(stream, 9);
            }
            else
            {
                ReadByteArray(stream, 5);
            }
        }

        public void ReadSpawnNamedEntity(MineStream stream)
        {
            ReadInt(stream);
            ReadString(stream);

            if(Global.CurrentProtocol > 79)
            {
                string s = ReadString(stream);
                //Console.WriteLine(s);
            }

            ReadByteArray(stream, 18);
            ReadMetaData(stream);
        }

        public void ReadChangeGameState(MineStream stream)
        {
            ReadByte(stream);

            if (Global.CurrentProtocol > 79)
            {
                ReadFloat(stream);
            }
            else
            {
                ReadByte(stream);
            }
        }

        public void ReadOpenWindow(MineStream stream)
        {
            ReadByte (stream);
            byte windowType = ReadByte (stream);
            ReadString (stream);
            ReadByte (stream);

            if(Global.CurrentProtocol >= 61)
            {
                ReadBool(stream);
            }

            if(Global.CurrentProtocol >= 72 && windowType == 0xb)
            {
                ReadInt(stream);
            }
        }

        private float ReadFloatList(List<byte> l)
        {
            byte[] buffer = l.ToArray();
            Array.Reverse(buffer);
            float final = BitConverter.ToSingle(buffer, 0);
            return final;
        }

        public void ReadPlayerPositionAndLook(MineStream stream)
        {

            ReadByteArray(stream, 41, true);

            /*
            byte[] b = ReadByteArray(stream, 41, true);
            Console.WriteLine(BitConverter.ToString(b));

            //double x = ReadDouble(stream);
            //double stance = ReadDouble(stream);
            //double y = ReadDouble(stream);
            //double z = ReadDouble(stream);
            //float yaw = ReadFloat(stream);
            //float pitch = ReadFloat(stream);
            //bool onGround = ReadBool(stream);

            List<byte> lb = new List<byte>();
            lb.AddRange(b);

            List<byte> listTemp = new List<byte>();
            listTemp.AddRange(lb.GetRange(0, 8));
            double x = ReadDoubleList(listTemp);

            listTemp.Clear();
            listTemp.AddRange(lb.GetRange(8, 8));
            double stance = ReadDoubleList(listTemp);

            listTemp.Clear();
            listTemp.AddRange(lb.GetRange(16, 8));
            double y = ReadDoubleList(listTemp);

            listTemp.Clear();
            listTemp.AddRange(lb.GetRange(24, 8));
            double z = ReadDoubleList(listTemp);

            listTemp.Clear();
            listTemp.AddRange(lb.GetRange(32, 4));
            float yaw = ReadFloatList(listTemp);

            listTemp.Clear();
            listTemp.AddRange(lb.GetRange(36, 4));
            float pitch = ReadFloatList(listTemp);

            bool onGround = Convert.ToBoolean(lb[40]);

            List<byte> ground = new List<byte>();

            // Respond exactly back
            ground.Add(0x0D);
            ground.AddRange(BitConverter.GetBytes(x).Reverse());
            ground.AddRange(BitConverter.GetBytes(y).Reverse());
            ground.AddRange(BitConverter.GetBytes(stance).Reverse());
            ground.AddRange(BitConverter.GetBytes(z).Reverse());
            ground.AddRange(BitConverter.GetBytes(yaw).Reverse());
            ground.AddRange(BitConverter.GetBytes(pitch).Reverse());
            ground.Add(Convert.ToByte(onGround));

            Console.WriteLine(BitConverter.ToString(ground.ToArray()));

            stream.Write(ground.ToArray(), 0, ground.Count);

            //stance = 1000;
            // Respond exactly back
            //ground.Clear();
            //ground.Add(0x0B);
            //ground.AddRange(BitConverter.GetBytes(x).Reverse());
            //ground.AddRange(BitConverter.GetBytes(y).Reverse());
            //ground.AddRange(BitConverter.GetBytes(stance).Reverse());
            //ground.AddRange(BitConverter.GetBytes(z).Reverse());
            //ground.Add(Convert.ToByte(1));
            
            //stream.Write(ground.ToArray(), 0, ground.Count);
            /*

            if(!onGround)
            {
                Console.WriteLine("Flying");



                List<byte> ground = new List<byte>();

                ground.Add(0x0D);
                ground.AddRange(BitConverter.GetBytes(x));
                ground.AddRange(BitConverter.GetBytes(y));
                ground.AddRange(BitConverter.GetBytes(stance));
                ground.AddRange(BitConverter.GetBytes(z));
                ground.AddRange(BitConverter.GetBytes(yaw));
                ground.AddRange(BitConverter.GetBytes(pitch));
                ground.Add(Convert.ToByte(onGround));
                stream.Write(ground.ToArray(), 0, ground.Count);


                List<byte> aground = new List<byte>();
                Console.WriteLine("Flying");
                aground.Add(0x0A);
                aground.Add(Convert.ToByte(true));
                stream.Write(aground.ToArray(), 0, aground.Count);


            }

            List<byte> aground = new List<byte>();

            aground.Add(0x0A);
            aground.Add(Convert.ToByte(1));
            stream.Write(aground.ToArray(), 0, aground.Count);
            */
        }

        public void ReadBlockAction(MineStream stream)
        {
            ReadByteArray(stream, 14);
        }

        public void ReadEntityLook(MineStream stream)
        {
            ReadByteArray(stream, 6);
        }

        public void ReadUpdateScore(MineStream stream)
        {
            ReadString(stream, true);
            byte updateRemove = ReadByte(stream);
            if(updateRemove == 0)
            {
                ReadString(stream, false);
                ReadInt(stream);
            }
        }

        public void ReadChunkData(MineStream stream)
        {
            ReadByteArray(stream, 13);
            int length = ReadInt(stream);
            ReadByteArray(stream, length, false); 
        }

        public void ReadPluginMessage(MineStream stream)
        {
            try
            {
                string x = ReadString(stream, true);
                //Console.WriteLine ("Plugin: " + x);


                short length = ReadShort(stream);
                //Console.WriteLine(length.ToString());


                byte[] q = ReadByteArray(stream, (int)length, true);

                if(x=="FML")
                {
                    //string h = ReadString(stream, true);
                    string t = Encoding.UTF8.GetString(q, 0, q.Length);
                    //Console.WriteLine("h");

                }
            }
            catch(Exception ex) {
                //Console.WriteLine (ex.Message);
            }
        }

        public void ReadSpawnMob(MineStream stream)
        {
            ReadByteArray(stream, 26, false);
            ReadMetaData(stream);
        }

        public void ReadPlayer(MineStream stream)
        {
            //Global.IsOnGround = ReadBool(stream); 
            ReadBool(stream); 
        }

        public void ReadPlayerAbilities(MineStream stream)
        {

            if(Global.CurrentProtocol > 62)
            {
                ReadByteArray(stream, 9);
            }
            else
            {
                ReadByteArray(stream, 3);
            }
        }


        public void ReadIncrementStatistic(MineStream stream)
        {
            string s = string.Empty;

            if (Global.CurrentProtocol > 78)
            {
                int count = ReadInt (stream);
                for(int i=0; i < count; i++)
                {
                    s = ReadString(stream);
                    ReadInt (stream);
                }
            }
            else if(Global.CurrentProtocol > 62)
            {
                ReadInt (stream);
                ReadInt (stream);
            }
            else
            {
                ReadInt (stream);
                ReadByte (stream);
            }

        }


        public void ReadParticle(MineStream stream)
        {
            ReadString(stream);
            ReadByteArray (stream, 32);

        }

        public void ReadEntityProperties(MineStream stream)
        {

            ReadInt (stream);
            int count = ReadInt (stream);
            //Console.WriteLine ("COUNT:" + count.ToString ());

            for(int i=0; i < count; i++)
            {
                string s = ReadString (stream, true);
                //Console.WriteLine (s);
                ReadDouble (stream);

                if (Global.CurrentProtocol > 73) 
                {
                    int shortCount = ReadShort (stream);
                    //Console.WriteLine ("shortCOUNT:" + shortCount.ToString ());
                    if(shortCount > 0)
                    {
                        for (int a=0; a < shortCount; a++) 
                        {
                            ReadLong (stream);
                            ReadLong (stream);
                            ReadDouble (stream);
                            ReadByte (stream);
                        }
                    }
                }
            }

        }

        public void ReadAttachEntity(MineStream stream)
        {

            if(Global.CurrentProtocol > 62)
            {
                ReadByteArray(stream, 9);
            }
            else
            {
                ReadByteArray(stream, 8);
            }
        }

        public void ReadUpdateHealth(MineStream stream)
        {

            if(Global.CurrentProtocol > 62)
            {
                ReadFloat(stream);
            }
            else
            {
                ReadShort(stream);
            }

            //short health = ReadShort(stream);

            short food = ReadShort(stream);
            ReadFloat(stream);

            //Console.WriteLine("Packet: " + playerName + " " + online.ToString());
            //if (PlayerOnline != null)
            //{
            //    PlayerOnline(playerName, online);
            //}
        }

        public void ReadPlayerListItem(MineStream stream)
        {
            string playerName = ReadString(stream, true);
            bool online = ReadBool(stream);
            ReadShort(stream);
            //Console.WriteLine("Packet: " + playerName + " " + online.ToString());
            if (PlayerOnline != null)
            {
                PlayerOnline(playerName, online, string.Empty);
            }
        }

        public void ReadSetWindowItems(MineStream stream)
        {
            ReadByte(stream);
            short items = ReadShort(stream);
            for (int i = 0; i < items; i++)
            {
                ReadSlot(stream);
            }
        }      

        public double ReadDouble(MineStream stream)
        {
            byte[] buffer = stream.Read(0, 8);
            Array.Reverse(buffer);
            double final = BitConverter.ToDouble(buffer, 0);
            return final;
        }


        public void ReadMultipleBlockChange(MineStream stream)
        {
            ReadInt(stream);
            ReadInt(stream);
            ReadShort(stream);
            int dataSize = ReadInt(stream);
            ReadByteArray(stream, (int)dataSize);
        }

        public float ReadFloat(MineStream stream)
        {
            byte[] buffer = stream.Read(0, 4);
            Array.Reverse(buffer);
            float final = BitConverter.ToSingle(buffer, 0);
            return final;
        }

        private void ReadDisconnected(MineStream stream)
        {
            string reason = ReadString(stream, true);
            if (Disconnected != null)
            {
                Disconnected(reason);
            }
        }

        public void ProcessKeepAlive(MineStream stream)
        {
            int id = ReadInt(stream);
            byte[] returnID = BitConverter.GetBytes(id);

            //Console.WriteLine("****************Keep Alive******************");

            List<byte> keepAlive = new List<byte>();
            keepAlive.Add(0x00);
            keepAlive.AddRange(returnID);
            stream.Write(keepAlive.ToArray(), 0, keepAlive.Count);

            if(KeepAlive!=null)
            {
                KeepAlive();
            }
        }
    }
}
