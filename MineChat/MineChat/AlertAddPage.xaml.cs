using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Xamarin.Forms;
using Acr.UserDialogs;
using MineChat.Languages;

namespace MineChat
{
    public partial class AlertAddPage : ContentPage
    {
        private bool save = false;

        public AlertAddPage()
        {
            InitializeComponent();
            this.Title = AppResources.minechat_general_alerts;
            entryWord.Text = AppResources.minechat_general_alertword;
            labelSound.Text = AppResources.minechat_general_sound;
            labelVibrate.Text = AppResources.minechat_general_vibrate;
            //switchSound.Text = AppResources.minechat_general_sound;
            //switchVibrate.Text = AppResources.minechat_general_vibrate;

            SetupPage();
        }

        public bool Save
        {
            get
            {
                return save;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Save_Clicked(object sender, EventArgs e)
        {
            string errorMessage = string.Empty;
            
            if(entryWord.Text==null || entryWord.Text.Trim() == string.Empty)
            {
                ShowMessage(AppResources.minechat_general_alertblank);
                return;
            }

            save = true;
            Close();
        }

        private void ShowMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Add Alert Failed", message, "Ok");
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


