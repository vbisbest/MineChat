using System;
using UIKit;
using Foundation;
using AudioToolbox;
using MessageUI;
using MineChat;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.iOS.Device))]
namespace MineChat.iOS
{
    public class Device : IDevice
    {

        public bool IsLite()
        {
            if (NSBundle.MainBundle.BundleIdentifier == "com.kellyproductions.minechatlite")
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        public string ProductName()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleDisplayName"].ToString(); 
        }

        public string AppCenterId()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                if (IsLite())
                {
                    return "622eafd9-28c0-47c4-889f-fd3fbf17ad6e";
                }
                else
                {
                    return "0b66d477-54be-407e-932a-42a925776871";
                }
            }
            else
            {
                return null;
            }
        }

        public void Sound()
        {
            try
            {
                SystemSound sound = SystemSound.FromFile("beep.wav");
                sound.PlaySystemSound();
            }
            catch
            {
                // ignore
            }
        }
        
        public void Vibrate()
        {
            try
            {
                SystemSound.Vibrate.PlaySystemSound();
            }
            catch
            {
                // ignore
            }
        }

        public string Model()
        {
            return "iOS";
        }

        public string AppVersionName()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
        }

        public void OpenLink(string url)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
        }

        public void OpenFacebook()
        {
            if (UIApplication.SharedApplication.CanOpenUrl(new NSUrl("fb://profile/328970917219274")))
            {
                UIApplication.SharedApplication.OpenUrl((new NSUrl("fb://profile/328970917219274")));
            }
            else
            {
                OpenLink("https://www.facebook.com/minechatapp/");
            }
        }

        public void SendEmail(string[] to, string subject, string body)
        {
            try
            {
                MFMailComposeViewController mailController = new MFMailComposeViewController();
                mailController.SetToRecipients(to);
                mailController.SetSubject(subject);
                mailController.SetMessageBody(body, true);
                mailController.Finished += (object s, MFComposeResultEventArgs args) =>
                {
                    Console.WriteLine(args.Result.ToString());
                    args.Controller.DismissViewController(true, null);
                };

                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(mailController, true, null);
            }
            catch
            {
                // ignore
            }
        }

        public void CopyToClipboard(string text)
        {
            
            UIPasteboard.General.SetValue(new NSString(text), "public.utf8-plain-text");
        }

        public void Log(string message)
        {
            throw new NotImplementedException();
        }
    }
}
