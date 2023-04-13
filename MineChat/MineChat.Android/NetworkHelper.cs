using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using MineChat;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net;
using Java.Net;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.Droid.NetworkHelper))]
namespace MineChat.Droid
{
    public class NetworkHelper : INetworkHelper
    {
        public List<IPEndPoint> GetDnsServers()
        {
            try
            {
                List<IPEndPoint> endPoints = new List<IPEndPoint>();
                ConnectivityManager connectivityManager = (ConnectivityManager)GlobalDroid.mainActivity.GetSystemService(MainActivity.ConnectivityService);

                Network activeConnection = connectivityManager.ActiveNetwork;
                var linkProperties = connectivityManager.GetLinkProperties(activeConnection);

                foreach (InetAddress currentAddress in linkProperties.DnsServers)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(currentAddress.HostAddress), 53);
                    endPoints.Add(endPoint);
                }

                return endPoints;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
