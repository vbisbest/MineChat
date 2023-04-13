using System;
using Android.Net;
using System.Net;
using System.Net.NetworkInformation;
using Android.Net.Wifi;
using Android.App;
using Android.Content;
using Android.Telephony;

namespace MineChat.Droid
{
    public class Connectivity 
    {


        /**
     * Get the network info
     * @param context
     * @return
     */
        public static NetworkInfo GetNetworkInfo(Context context)
        {
            try
            {
                ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Service.ConnectivityService);
                return cm.ActiveNetworkInfo;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /**
     * Check if there is any connectivity
     * @param context
     * @return
     */
        public static bool IsConnected(Context context)
        {
            NetworkInfo info = Connectivity.GetNetworkInfo(context);
            return (info != null && info.IsConnected);
        }

        /**
     * Check if there is any connectivity to a Wifi network
     * @param context
     * @param type
     * @return
     */
        public static bool IsConnectedWifi(Context context)
        {            
            NetworkInfo info = Connectivity.GetNetworkInfo(context);
            return (info != null && info.IsConnected && info.Type == ConnectivityType.Wifi);
        }

        /**
     * Check if there is any connectivity to a mobile network
     * @param context
     * @param type
     * @return
     */
        public static bool IsConnectedMobile(Context context)
        {
            NetworkInfo info = Connectivity.GetNetworkInfo(context);
            return (info != null && info.IsConnected && info.Type == ConnectivityType.Mobile);
        }

        /**
     * Check if there is fast connectivity
     * @param context
     * @return
     */
        public static bool IsConnectedFast(Context context)
        {
            NetworkInfo info = Connectivity.GetNetworkInfo(context);
            return (info != null && info.IsConnected && Connectivity.IsConnectionFast(info.Type, info.Subtype));
        }

        /**
     * Check if the connection is fast
     * @param type
     * @param subType
     * @return
     */
        public static bool IsConnectionFast(ConnectivityType type, ConnectivityType subType){
            if (type == ConnectivityType.Wifi)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

