using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Xamarin.Forms;
using Acr.UserDialogs;
using MineChat.Languages;
using MineChatAPI;

namespace MineChat
{
    public partial class ServerAddPage : ContentPage
    {
        private bool save = false;

        public ServerAddPage()
        {
            InitializeComponent();
            SetupPage();
        }

        public bool Save
        {
            get
            {
                return save;
            }
        }

        private void SetupPage()
        {
            try
            {
                ToolbarItem save = new ToolbarItem();

                save.Clicked += Save_Clicked;
                save.Text = AppResources.minechat_general_save;
                save.Icon = "save.png";

                this.ToolbarItems.Add(save);

                labelServerAddress.Text = AppResources.minechat_settings_address;
                labelServerName.Text = AppResources.minechat_settings_servername;
                labelVersion.Text = AppResources.selectWorld_version;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            var server = (Server)this.BindingContext;

            string errorMessage = string.Empty;
            Uri uri = null;

            try
            {
                uri = new Uri("http://" + server.FullAddress);
            }
            catch(Exception ex)
            {
                errorMessage = AppResources.minechat_settings_errorinvalidaddress;
            }
            
            if(uri==null)
            {
                errorMessage = AppResources.minechat_settings_errorinvalidaddress;
            }
            else if (server.ServerName == string.Empty)
            {
                errorMessage = AppResources.minechat_settings_errornameblank;
            }
            else if (uri.Host == string.Empty)
            {
                errorMessage = AppResources.minechat_settings_erroraddressblank;
            }

            if(errorMessage!=string.Empty)
            {
                ShowMessage(errorMessage);
                return;
            }

            save = true;
            Close();
        }

        private void ShowMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Add Server Failed", message, "Ok");
            });
        }

        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private async void Close()
        {
            await Navigation.PopAsync();
        }
    }
}


