using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MineChat.iOS
{
    public class SrvResolver
    {
        [DllImport("libresolv", EntryPoint="__res_query")]
        private static extern int linux_res_query (string dname, int cls, int type, byte[] header, int headerlen);

        [DllImport("libresolv", EntryPoint="__dn_expand")]
        private unsafe static extern int linux_dn_expand (byte* msg, byte* endorig, byte* comp_dn, byte[] exp_dn, int length);

        [DllImport("libresolv", EntryPoint="res_query")]
        private static extern int bsd_res_query (string dname, int cls, int type, byte[] header, int headerlen);

        [DllImport("libresolv", EntryPoint="dn_expand")]
        private unsafe static extern int bsd_dn_expand (byte* msg, byte* endorig, byte* comp_dn, byte[] exp_dn, int length);

        private unsafe static int res_query (string dname, int cls, int type, byte[] header, int headerlen)
        {
            try {
                return linux_res_query(dname, cls, type, header, headerlen);
            } catch (EntryPointNotFoundException) {
                return bsd_res_query(dname, cls, type, header, headerlen);
            }
        }

        private unsafe static int dn_expand (byte* msg, byte* endorig, byte* comp_dn, byte[] exp_dn, int length)
        {
            try {
                return linux_dn_expand(msg, endorig, comp_dn, exp_dn, length);
            } catch (EntryPointNotFoundException) {
                return bsd_dn_expand(msg, endorig, comp_dn, exp_dn, length);
            }
        }

        [DllImport("libc")]
        private static extern UInt16 ntohs(UInt16 netshort);

        private static readonly int C_IN  = 1;
        private static readonly int T_SRV = 33;

        [StructLayout(LayoutKind.Explicit)]
        private struct HEADER
        {
            /* The first 4 bytes are a bunch of random crap that
             * nobody cares about */

            [FieldOffset(4)]
            public UInt16 qdcount; /* number of question entries */

            [FieldOffset(6)]
            public UInt16 ancount; /* number of header entries */

            [FieldOffset(8)]
            public UInt16 nscount; /* number of authority entries */

            [FieldOffset(10)]
            public UInt16 arcount; /* number of resource entries */
        }

        private unsafe static ushort GETSHORT (ref byte* buf)
        {
            byte *t_cp = (byte*)(buf);
            ushort s = (ushort) (((ushort)t_cp[0] << 8) | ((ushort)t_cp[1]));
            buf += sizeof(ushort);
            return s;
        }

        public static SrvRecord[] Resolve (string query)
        {
            byte[] buffer = new byte[1024];
            byte[] name = new byte[256];
            ushort type, dlen, priority, weight, port;
            int size;
            GCHandle handle;
            List<SrvRecord> results = new List<SrvRecord>();

            size = res_query(query, C_IN, T_SRV, buffer, buffer.Length);

            handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try {
                HEADER header = (HEADER)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HEADER));

                int qdcount = ntohs(header.qdcount);
                int ancount = ntohs(header.ancount);

                int headerSize = Marshal.SizeOf(header);

                unsafe {
                    fixed (byte* pBuffer = buffer) {

                        byte *pos = pBuffer + headerSize;
                        byte *end = pBuffer + size;

                        // We don't care about the question section.
                        while (qdcount-- > 0 && pos < end) {
                            size = dn_expand(pBuffer, end, pos, name, 256);
                            if (size < 0) return null;
                            pos += size + 4;
                        }

                        // The answers, however, we do care about!
                        while (ancount-- > 0 && pos < end) {
                            size = dn_expand(pBuffer, end, pos, name, 256);
                            if (size < 0) return null;

                            pos += size;

                            type = GETSHORT(ref pos);

                            // Skip TTL
                            pos += 6;

                            dlen = GETSHORT(ref pos);

                            if (type == T_SRV) {
                                priority = GETSHORT(ref pos);
                                weight = GETSHORT(ref pos);
                                port = GETSHORT(ref pos);

                                size = dn_expand(pBuffer, end, pos, name, 256);
                                if (size < 0) return null;

                                string nameStr = null;
                                fixed (byte* pName = name) {
                                    nameStr = new String((sbyte*)pName);
                                }

                                results.Add(new SrvRecord(nameStr, port, priority, weight));

                                pos += size;
                            } else {
                                pos += dlen;
                            }
                        }
                    }
                }

            } finally {
                handle.Free();
            }

            return results.ToArray();
        }
    }

    public class SrvRecord
    {
        string m_Name;
        int    m_Priority;
        int    m_Port;
        int    m_Weight;

        public SrvRecord (string name, int port, int priority, int weight)
        {
            m_Name   = name;
            m_Port   = port;
            m_Priority   = priority;
            m_Weight = weight;
        }

        public string Name {
            get {
                return m_Name;
            }
        }

        public int Port {
            get {
                return m_Port;
            }
        }

        public int Priority {
            get {
                return m_Priority;
            }
        }

        public int Weight {
            get {
                return m_Weight;
            }
        }

        public override string ToString ()
        {
            return String.Format("{0} {1} {2} {3}", m_Priority, m_Weight, m_Port, m_Name);
        }
    }
}