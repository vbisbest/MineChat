using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace MineChatAPI
{
    public class PacketReader
    {
        private Stream stream;
        private int timeout = 0;
        Stopwatch sw = new Stopwatch();

        public PacketReader()
        {
        }

        public PacketReader(int timeout)
        {
            stream = new MemoryStream();
            this.timeout = timeout;
        }

        public void Position(int i)
        {
            this.Position(i);
        }

        public byte[] ReadToEnd()
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public void SetCurrentPacket(byte[] bytes)
        {
            this.stream = new MemoryStream(bytes);
        }

        public void SetCurrentPacket(Stream stream)
        {
            this.stream = stream;
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

        public double ReadDouble()
        {
            byte[] buffer = new byte[8];
            this.Read(buffer, 0, buffer.Length);
            Array.Reverse((Array)buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        public float ReadFloat()
        {
            byte[] buffer = new byte[4];
            this.Read(buffer, 0, buffer.Length);
            Array.Reverse((Array)buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public short ReadShort()
        {
            byte[] buffer = new byte[2];
            this.Read(buffer, 0, buffer.Length);
            Array.Reverse((Array)buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        public byte[] ReadByteArray(int length)
        {
            byte[] buffer = new byte[length];
            if (length == 0)
                return buffer;
            this.Read(buffer, 0, length);
            return buffer;
        }

        public byte ReadByte()
        {
            byte[] buffer = new byte[1];
            this.Read(buffer, 0, 1);
            return buffer[0];
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            int count = length;

            sw.Reset();
            sw.Start();

            try
            {
                while (count != 0)
                {
                    count -= stream.Read(buffer, length - count, count);
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                throw new Exception("Read failed");
            }

            sw.Stop();
            return length;
        }

        public string ReadStringClassic()
        {
            byte[] numArray = new byte[this.ReadShort()];

            this.Read(numArray, 0, numArray.Length);
            return Encoding.UTF8.GetString(numArray, 0, numArray.Length);
        }

        public string ReadString()
        {
            byte[] numArray = new byte[this.ReadRawVarint32()];

            this.Read(numArray, 0, numArray.Length);
            return Encoding.UTF8.GetString(numArray, 0, numArray.Length);
        }

        public string ReadUUID()
        {
            byte[] buffer = new byte[16];
            this.Read(buffer, 0, buffer.Length);
            Array.Reverse((Array)buffer);            
            //return BitConverter.ToInt16(buffer, 0);
            return new Guid(buffer).ToString();
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
            int bytesOut = 0;
            return ReadRawVarint32(out bytesOut);
        }

        public int ReadInt()
        {
            byte[] buffer = new byte[4];
            this.Read(buffer, 0, buffer.Length);
            Array.Reverse((Array)buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        public uint ReadUnsignedInt()
        {
            byte[] buffer = new byte[4];
            this.Read(buffer, 0, buffer.Length);
            Array.Reverse((Array)buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public long ReadLong()
        {
            byte[] buffer = new byte[8];
            this.Read(buffer, 0, buffer.Length);
            Array.Reverse((Array)buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public sbyte ReadUnsignedByte()
        {
            return (sbyte)this.ReadByte();
        }

        public long ReadUnsignedByteArray(MineStream stream)
        {
            return 0L;
        }

        public bool ReadBool()
        {
            return (int)this.ReadByte() == (int)Convert.ToByte(1);
        }

        public ushort ReadUnsingedShort()
        {
            byte[] buffer = new byte[2];
            this.Read(buffer, 0, buffer.Length);
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
