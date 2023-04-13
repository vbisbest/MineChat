using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MineChat
{
    public class Product
    {
        
        public static string AnalyticsID = "redacted";
        //public const string ProductName = "MineChat";
        public const string AppodealKey = "";
        public const string ApiKey = "redacted";

        public static string ProductName
        {
            get
            {
                var device = DependencyService.Get<IDevice>();
                if (device != null)
                {
                    return device.ProductName();
                }
                else
                {
                    return string.Empty;
                }
            }

        }

        public static bool IsLite
        {
            get
            {
                var device = DependencyService.Get<IDevice>();
                if (device != null)
                {
                    return device.IsLite();
                }
                else
                {
                    return true;
                }
            }

        }

        public static string Platform
        {
            get
            {                
                if (Device.OS == TargetPlatform.Windows)
                {
                    return "Windows 10";
                }
                else if (Device.OS == TargetPlatform.Android)
                {
                    return "Android";
                }

                return string.Empty;
            }
        }


        public static int PlatformId
        {
            get
            {
                if (Device.OS == TargetPlatform.Windows)
                {
                    return 30;
                }
                else if (Device.OS == TargetPlatform.Android)
                {
                    return 20;
                }

                return 10;
            }
        }

    }
}
