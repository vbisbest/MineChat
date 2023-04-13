using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net;
using MineChat;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.iOS.NetworkHelper))]
namespace MineChat.iOS
{
    public class NetworkHelper : INetworkHelper
    {
        private int maxDNS = 3;

        res_state state = new res_state();
        res_sockaddr_union union = new res_sockaddr_union();

        [DllImport("libresolv", CallingConvention = CallingConvention.Cdecl, EntryPoint = "res_9_ninit")]
        private static extern int res_ninit(IntPtr state);

        [DllImport("libresolv", CallingConvention = CallingConvention.Cdecl, EntryPoint = "res_9_getservers")]
        private static extern int res_getservers(IntPtr state, IntPtr union, ref int count);

        [DllImport("libresolv", EntryPoint = "res_9_init")]
        private static extern int res_init();

        public List<IPEndPoint> GetDnsServers()
        {
            GCHandle stateHandle = new GCHandle();
            //GCHandle dnsServersHandle = new GCHandle();

            List<IPEndPoint> result = new List<IPEndPoint>();

            try
            {
                // Get state
                stateHandle = GCHandle.Alloc(state, GCHandleType.Pinned );
                int r = res_ninit(stateHandle.AddrOfPinnedObject());

                if (r != 0)
                {
                    throw new Exception("DNS failed");
                }

                state = (res_state)Marshal.PtrToStructure(stateHandle.AddrOfPinnedObject(), typeof(res_state));

                /*
                // Get DNS servers
                dnsServersHandle = GCHandle.Alloc(union, GCHandleType.Pinned);
                res_getservers(stateHandle.AddrOfPinnedObject(), dnsServersHandle.AddrOfPinnedObject(), ref maxDNS);

                int offset = 0;

                for (int i = 0; i < maxDNS; ++i)
                {
                    //Console.WriteLine("Offset: " + offset + ":" + i);                   
                    sockaddr currentServer = (sockaddr)Marshal.PtrToStructure(IntPtr.Add(dnsServersHandle.AddrOfPinnedObject(), offset), typeof(sockaddr));
                    
                    if (currentServer.sa_family == SockAddrFamily.Inet)
                    {
                        sockaddr_in dnsServerDetails = (sockaddr_in)Marshal.PtrToStructure(IntPtr.Add(dnsServersHandle.AddrOfPinnedObject(), offset), typeof(sockaddr_in));
                        result.Add(GetEndPoint(dnsServerDetails));
                    }
                    else if (currentServer.sa_family == SockAddrFamily.Inet6)
                    {
                        //sockaddr_in6 dnsServerDetails = (sockaddr_in6)Marshal.PtrToStructure(IntPtr.Add(dnsServersHandle.AddrOfPinnedObject(), offset), typeof(sockaddr_in6));
                        //IPEndPoint e = GetEndPoint(dnsServerDetails);
                        Console.WriteLine("IPv6");
                    }
                    else
                    {
                        // we dont care about it
                        // Console.WriteLine("asdfasdf");
                    }

                    // Set the next memory position
                    offset += 128
                }
                */

                if(state.socket_in.sin_family != 0)
                {
                    result.Add(GetEndPoint(state.socket_in));
                }
                if (state.socket_in2.sin_family != 0)
                {
                    result.Add(GetEndPoint(state.socket_in2));
                }
                if (state.socket_in3.sin_family != 0)
                {
                    result.Add(GetEndPoint(state.socket_in3));
                }
            }
            catch (Exception ex)
            {
                // ignore
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //dnsServersHandle.Free();
                stateHandle.Free();
            }

            return result;
        }

        //private IPEndPoint GetEndPoint(sockaddr_in socket)
        //{
        //    IPEndPoint endPoint = null;
        //    System.Net.IPAddress ipAddress = new System.Net.IPAddress(socket.sin_addr);
        //    byte[] backwardsPort = BitConverter.GetBytes(socket.sin_port);
        //    Array.Reverse(backwardsPort);
        //    UInt16 port = BitConverter.ToUInt16(backwardsPort, 0);
        //    endPoint = new IPEndPoint(ipAddress, port);

        //    //Console.WriteLine(endPoint.Address.ToString());

        //    return endPoint;
        //}
    }

    [StructLayout(LayoutKind.Explicit, Size = 384)]
    public struct res_sockaddr_union
    {
        // Both members located on the same
        // position in the beginning of the union
        [FieldOffset(0)]
        public sockaddr_in socket_in;

        [FieldOffset(0)]
        public sockaddr_in6 socket_in6;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct res_state
    {
        public int retrans;
        public int retry;
        public ulong options;
        public int nameserversCount;
        public sockaddr_in socket_in;
        public sockaddr_in socket_in2;
        public sockaddr_in socket_in3;
    }

    public enum SockAddrFamily : byte
    {
        Inet = 0x2,
        Inet6 = 0x30
    }

    internal struct sockaddr
    {
        public byte sa_len;
        public SockAddrFamily sa_family;
    }
    
    //public unsafe struct sockaddr_in
    //{
    //    public byte sin_len;
    //    public byte sin_family;
    //    public ushort sin_port;
    //    public uint sin_addr;
    //    public fixed byte sin_zero[8];
    //}

    public struct sockaddr_in6
    {
        public byte sin6_len;
        public byte sin6_family;
        //public ushort sin6_port;
        //public uint sin6_flowinfo;
        //public in6_addr sin6_addr;
        //public uint sin6_scope_id;
    }

    public struct in6_addr
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] u6_addr8;
    }
}
 