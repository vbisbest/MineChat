using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Resources;
using System.Globalization;
using MineChat.Languages;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;

namespace MineChat
{
    public class App : Application
    {
        public App()
        {
            try
            {
                var l = DependencyService.Get<ILocalize>();
                
                if (l != null)
                {
                    var netLanguage = l.GetCurrentCultureInfo();
                    AppResources.Culture = netLanguage;
                }
            }
            catch(Exception ex)
            {
                // Something went wrong, just log it and use english

            }

            try
            {
                Global.LoadSettings();
                MainPage = new MineChat.MainPage();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().Message);
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            Debug.WriteLine("onstart");
            try
            {
                var device = DependencyService.Get<IDevice>();
                if (device != null)
                {
                    string appId = device.AppCenterId();
                    if (appId != null)
                    {
                        AppCenter.Start(appId, typeof(Analytics), typeof(Crashes));
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Ignore
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            Debug.WriteLine("sleep");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            Debug.WriteLine("resume");
        }

        
    }
}
