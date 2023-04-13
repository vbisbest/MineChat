using System;
using Xamarin.Forms;
using System.Threading;

[assembly:Dependency(typeof(MineChat.Droid.Localize))]

namespace MineChat.Droid
{
	public class Localize : MineChat.ILocalize
	{
		public System.Globalization.CultureInfo GetCurrentCultureInfo ()
		{
            try
            {
                var androidLocale = Java.Util.Locale.Default;
                var netLanguage = androidLocale.ToString().Replace("_", "-");
                if (netLanguage == "iw-IL" || netLanguage == "iw")
                {
                    netLanguage = "he";
                    SetLocale();
                }

                return new System.Globalization.CultureInfo(netLanguage);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new System.Globalization.CultureInfo("en-US");


        }

		public void SetLocale ()
		{
			var androidLocale = Java.Util.Locale.Default; // user's preferred locale
			var netLocale = androidLocale.ToString().Replace ("_", "-");
            if (netLocale == "iw-IL" || netLocale == "iw")
            {
                netLocale = "he";
            }

            var ci = new System.Globalization.CultureInfo (netLocale);
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;
		}
	}
}

