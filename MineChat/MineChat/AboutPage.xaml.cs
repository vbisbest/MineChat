using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineChat.Languages;

using Xamarin.Forms;

namespace MineChat
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            
            this.Title = AppResources.minechat_settings_about;
            
            var device = DependencyService.Get<IDevice>();
            if (device != null)
            {
                labelVersion.Text = "Version " + device.AppVersionName();
            }
            

            labelWebsite.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnWebsiteClicked()),
            });

            labelFacebook.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnFacebookClicked()),
            });

            labelTwitter.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnTwitterClicked()),
            });

            labelEmail.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnEmailClicked()),
            });

            //labelPromote.GestureRecognizers.Add(new TapGestureRecognizer
            //{
            //    Command = new Command(() => OnPromoteClicked()),
            //});
            
            //var htmlSource = new HtmlWebViewSource();
            //htmlSource.Html = "<html><body>test<script src=\"http://z-na.amazon-adsystem.com/widgets/onejs?MarketPlace=US&adInstanceId=92095b57-62f8-4279-af7d-0e59db0250d9\"  type=\"text/javascript\"></script></html></body>";
            //Browser.Source = htmlSource;

            
        }
        
        private void OnWebsiteClicked()
        {
            var device = DependencyService.Get<IDevice>();
            if (device != null)
            {
                device.OpenLink("http://www.minechatapp.com");
            }
        }

        private void OnEmailClicked()
        {
            var device = DependencyService.Get<IDevice>();
            if (device != null)
            {
                string[] to = new string[] { "support@minechatapp.com" };
                string subject = Product.ProductName + " Support Request";
                string body = "Version: " + device.AppVersionName() + "\r\n";
                body += "Device: " + device.Model() + "\r\n";
                body += "Please include all details of your issue here including server address and error messages\r\n\r\n";
                device.SendEmail(to, subject, body);
            }
        }

        private void OnTwitterClicked()
        {
            var device = DependencyService.Get<IDevice>();
            if (device != null)
            {
                device.OpenLink("https://twitter.com/MineChatApp");
            }
        }

        private void OnPromoteClicked()
        {
            var device = DependencyService.Get<IDevice>();
            if (device != null)
            {
                device.OpenLink("http://www.minechatapp.com/featured-servers/");
            }
        }

        private void OnFacebookClicked()
        {
            var device = DependencyService.Get<IDevice>();
            if (device != null)
            {
                device.OpenFacebook();
            }
        }
        
    }
}
