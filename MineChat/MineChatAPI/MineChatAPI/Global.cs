using System;
using System.Text;

namespace MineChatAPI
{
    public class Global
    {
        public static bool IsOffline = false;
        public static int CurrentProtocol = 0;
        public static Settings CurrentSettings;
        public static DateTime LastFeaturedServerUpdate;



        static Global()
        {
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("0x{0:x2}, ", b);
            return hex.ToString();
        }
    }

    public enum ChatType
    {
        ChatBox,
        System,
        ActionBar
    }
}
