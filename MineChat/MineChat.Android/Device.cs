using System;
using Android.Media;
using Android.App;
using Android.Content;
using Android.OS;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.Droid.Device))]
namespace MineChat.Droid
{
    public class Device : Java.Lang.Object, IDevice
    {

        public void SendEmail(string[] to, string subject, string body)
        {
            try
            {
                var email = new Intent(Intent.ActionSend);
                email.PutExtra(Intent.ExtraEmail, to);
                email.PutExtra(Intent.ExtraSubject, subject);
                email.PutExtra(Intent.ExtraText, body);
                email.PutExtra(Intent.ExtraHtmlText, true);
                email.SetType("message/rfc822");
                Xamarin.Forms.Forms.Context.StartActivity(email);
            }
            catch
            {
                // Ignore
            }
        }

        public void Sound()
        {
            MediaPlayer mp = MediaPlayer.Create(Application.Context, Resource.Raw.beep);
            mp.Start();
        }

        public void Vibrate()
        {
            Vibrator vibrator = (Vibrator)Application.Context.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(100);
        }

        public string AppVersionName()
        {
            var context = Xamarin.Forms.Forms.Context;
            return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
        }

        public void CopyToClipboard(string text)
        {
            // Get the Clipboard Manager
            var clipboardManager = (ClipboardManager)Xamarin.Forms.Forms.Context.GetSystemService(Context.ClipboardService);

            // Create a new Clip
            ClipData clip = ClipData.NewPlainText("MineChat", text);

            // Copy the text
            clipboardManager.PrimaryClip = clip;
        }

        public string Model()
        {
            try
            {
                return Build.Manufacturer.ToLower() + " " + Build.Model.ToLower();
            }
            catch
            {
                return "Android";
            }
        }

        public void OpenLink(string url)
        {
            try
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    Android.Net.Uri uri = Android.Net.Uri.Parse(url);
                    string u = uri.Scheme.ToLower() + ":" + uri.SchemeSpecificPart;
                    Intent link = new Intent(Intent.ActionView, Android.Net.Uri.Parse(u));
                    GlobalDroid.mainActivity.StartActivity(link);
                });
            }
            catch
            {
                // ignore
            }
        }

        public void Log(string message)
        {
            Android.Util.Log.WriteLine(Android.Util.LogPriority.Debug, "MineChat", message);
        }

        public void OpenFacebook()
        {
            Intent fb = null;
            try
            {
                GlobalDroid.mainActivity.PackageManager.GetPackageInfo("com.facebook.katana", 0);
                fb = new Intent(Intent.ActionView, Android.Net.Uri.Parse("fb://profile/328970917219274"));
            }
            catch (Exception e)
            {
                fb = new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://www.facebook.com/minechatapp"));
            }

            GlobalDroid.mainActivity.StartActivity(fb);
        }

        public bool IsLite()
        {
            var context = GlobalDroid.mainActivity.ApplicationContext;
            return context.PackageName == "com.kellyproductions.minechatlite";
        }

        public string AppCenterId()
        {
            if(IsLite())
            {
                return "08c3e8aa-81f5-4f68-ae9a-91f2e925d878";
            }
            else
            {
                return "a1936cb1-a652-4df6-95af-28d59be2fcd7";
            }
        }

        public string ProductName()
        {
            var context = GlobalDroid.mainActivity.ApplicationContext;
            return context.ApplicationInfo.LoadLabel(context.PackageManager);
        }

    }
}