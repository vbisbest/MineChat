using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Xamarin.Forms;
using MineChat.Languages;

namespace MineChat
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();


            this.Title = AppResources.minechat_general_settings;
            this.BindingContext = Global.CurrentSettings;

            labelFont.Text = AppResources.minechat_settings_font;
            labelSize.Text = AppResources.minechat_settings_fontsize;
            checkPromotedServers.Text = AppResources.minechat_settings_showpromoted;
            checkShowHeads.Text = AppResources.minechat_settings_showheads;
            checkSpawnOnConnect.Text = AppResources.minechat_settings_spawnonconnect;
            textLogonMessage.Label = AppResources.minechat_settings_logonmessage;
            checkAutoHideKeyboard.Text = AppResources.minechat_settings_autohidekeyboard;
            checkAutoCorrect.Text = AppResources.minechat_settings_autocorrect;

            SetupPage();
        }

        private void CheckPromotedServers_OnChanged(object sender, ToggledEventArgs e)
        {
            if (Product.IsLite)
            {
                if (e.Value == false)
                {
                    checkPromotedServers.On = true;
                    ShowMessage("This setting is only available in the full version of MineChat");
                }
            }
            else
            {
                Global.FeaturedServersChanged = true;
            }
        }

        private void SetupPage()
        {
            if(Product.IsLite)
            {
                textLogonMessage.IsEnabled = false;
            }
            else
            {
                textLogonMessage.Text = Global.CurrentSettings.LogonMessage;
            }

            pickerFont.SelectedIndex = Global.CurrentSettings.Font;
            pickerFontSize.SelectedIndex = Global.CurrentSettings.FontSize;
            checkSpawnOnConnect.On = Global.CurrentSettings.SpawnOnConnect;
            checkPromotedServers.On = Global.CurrentSettings.ShowFeaturedServers;
            checkAutoCorrect.On = Global.CurrentSettings.AutoCorrect;
            checkAutoHideKeyboard.On = Global.CurrentSettings.AutoHideKeyboard;

            checkPromotedServers.OnChanged += CheckPromotedServers_OnChanged;
            checkPromotedServers.Tapped += CheckPromotedServers_Tapped;

            if (Product.IsLite)
            {
                textLogonMessage.Label = string.Empty;
            }
        }

        private void CheckPromotedServers_Tapped(object sender, EventArgs e)
        {
            if (Product.IsLite)
            {
                ShowMessage("This setting is only available in the full version of MineChat");                
            }

        }

        protected override void OnAppearing()
        {
            Global.CurrentPromotedServers.Servers.Clear();
            base.OnAppearing();
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            SavePage();
        }

        private void SavePage()
        {
            
            Global.CurrentSettings.LogonMessage = textLogonMessage.Text;
            Global.CurrentSettings.Font = pickerFont.SelectedIndex;
            Global.CurrentSettings.FontSize = pickerFontSize.SelectedIndex;
            Global.CurrentSettings.SpawnOnConnect = checkSpawnOnConnect.On;
            Global.CurrentSettings.AutoCorrect = checkAutoCorrect.On;
            Global.CurrentSettings.AutoHideKeyboard = checkAutoHideKeyboard.On;
            Global.CurrentSettings.ShowFeaturedServers = checkPromotedServers.On;

            Global.SaveSettings();
           
        }

        protected override void OnDisappearing()
        {
            SavePage();
            base.OnDisappearing();
        }

        private void ShowMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Settings", message, AppResources.minechat_general_ok);
            });
        }


    }
}
