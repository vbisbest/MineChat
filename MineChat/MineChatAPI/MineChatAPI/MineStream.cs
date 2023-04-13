using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Diagnostics;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net.Sockets;

namespace MineChatAPI
{
    public class MineStream
    {
        //private Stream writeStream;
        //private Stream readStream;

        private System.Net.Sockets.NetworkStream stream;

        //private TcpSocketClient streamSocket;
        byte[] last16Bytes = new byte[16];

        private BufferedBlockCipher encryptCipher;
        private BufferedBlockCipher decryptCipher;

        private bool encryptionEnabled = false;
        public bool CompressionEnabled = false;

        private Stopwatch sw = new Stopwatch();
        
        public MineStream(NetworkStream streamSocket)
        {
            this.stream = streamSocket;
        }

        public void EnableEncryption(byte[] key)
        {
            this.encryptCipher = this.GetCipher(true, key);
            this.decryptCipher = this.GetCipher(false, key);

            encryptionEnabled = true;
        }

        private BufferedBlockCipher GetCipher(bool flag, byte[] key)
        {
            BufferedBlockCipher bufferedBlockCipher = new BufferedBlockCipher((IBlockCipher)new CfbBlockCipher((IBlockCipher)new AesFastEngine(), 8));
            bufferedBlockCipher.Init(flag, (ICipherParameters)new ParametersWithIV((ICipherParameters)new KeyParameter(key), key));

            return bufferedBlockCipher;
        }

        public async void Disconnect()
        {
            try
            {
                stream.Close();
                //await streamSocket.DisconnectAsync();
            }
            catch
            {
                // ignore
            }
        }

        private byte[] Encrypt(byte[] buffer)
        {
            return this.encryptCipher.ProcessBytes(buffer);
        }

        private byte[] Decrypt(byte[] buffer, int start, int length)
        {
            return this.decryptCipher.ProcessBytes(buffer, start, length);            
        }

        public short ReadShort()
        {
            byte[] buffer = this.Read(0, 2);
            Array.Reverse((Array)buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public byte[] ReadByteArray(int length)
        {
            return this.ReadByteArray(length, false);
        }

        public void SkipBytes(int length)
        {
            stream.Position += length;
        }

        public byte[] ReadByteArray(int length, bool decrypt)
        {
            return this.Read(0, length, decrypt);
        }

        public byte ReadByte()
        {
            return this.Read(0, 1, true)[0];
        }

        public byte[] Read(int offset, int length, bool decrypt)
        {
            byte[] buffer = new byte[length];
            int remainingBytes = length;

            sw.Reset();
            sw.Start();

            try
            {
                while (remainingBytes != 0)
                {
                    remainingBytes -= stream.Read(buffer, length - remainingBytes, remainingBytes);
                    if (sw.ElapsedMilliseconds > 45000)
                    {
                        throw new Exception();
                    }             
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                throw new Exception("Timeout reading from stream");
            }

            sw.Stop();

            if (this.encryptionEnabled)
            {
                try
                {
                    //Debug.WriteLine("Decrypt");
                    if (decrypt || length < 17)
                    {
                        return this.Decrypt(buffer, 0, length);
                    }
                    else
                    {
                       // Debug.WriteLine("Skip decrypt");
                        return this.Decrypt(buffer, buffer.Length - 16, 16);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            // return unecrypted buffer
            return buffer;
        }

        public byte[] Read(int offset, int length)
        {
            return this.Read(offset, length, true);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                if (this.encryptionEnabled)
                {
                    byte[] buffer1 = this.Encrypt(buffer);
                    stream.Write(buffer1, 0, buffer1.Length);
                    stream.Flush();
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        public string ReadString()
        {
            return this.ReadString(false);
        }

        public string ReadString(bool decrypt)
        {
            int len = this.ReadRawVarint32();
            byte[] bytes = this.Read(0, len, decrypt);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        
        public int ReadRawVarint32(out int bytesRead)
        {
            bytesRead = 0;
            bool more = true;
            int value = 0;
            int shift = 0;
            while (more)
            {
                byte lower7bits = this.ReadByte();
                more = (lower7bits & 128) != 0;
                value |= (lower7bits & 0x7f) << shift;
                shift += 7;
                bytesRead++;
            }

            return value;
            
        }

        public int ReadRawVarint32()
        {
            bool more = true;
            int value = 0;
            int shift = 0;
            while (more)
            {
                byte lower7bits = this.ReadByte();
                more = (lower7bits & 128) != 0;
                value |= (lower7bits & 0x7f) << shift;
                shift += 7;
            }

            return value;
        }

        public byte[] WriteRawVarint32(int value)
        {
            List<byte> list = new List<byte>();
            uint num = (uint)value;
            while (num >= 128U)
            {
                list.Add((byte)(num | 128U));
                num >>= 7;
            }
            list.Add((byte)num);
            return list.ToArray();
        }

        public int ReadInt()
        {
            byte[] buffer = this.Read(0, 4);
            Array.Reverse((Array)buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        public uint ReadUnsignedInt()
        {
            byte[] buffer = this.Read(0, 4);
            Array.Reverse((Array)buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public long ReadLong()
        {
            byte[] buffer = this.Read(0, 8);
            Array.Reverse((Array)buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public sbyte ReadUnsignedByte()
        {
            return (sbyte)this.ReadByte();
        }

        public bool ReadBool()
        {
            return (int)this.ReadByte() == (int)Convert.ToByte(1);
        }

        public ushort ReadUnsingedShort()
        {
            byte[] buffer = this.Read(0, 2);
            Array.Reverse((Array)buffer);
            return BitConverter.ToUInt16(buffer, 0);
        }

        public void ReadArrayOfInt()
        {
            byte num = this.ReadByte();
            for (int index = 0; index < (int)num; ++index)
                this.ReadInt();
        }
    }
}
