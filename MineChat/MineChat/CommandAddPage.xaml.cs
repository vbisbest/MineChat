using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MineChat.Languages;
using Xamarin.Forms;

namespace MineChat
{
    public partial class CommandAddPage : ContentPage
    {
        private bool save = false;

        public CommandAddPage()
        {
            InitializeComponent();

            labelCommand.Text = AppResources.minechat_general_command;
            labelCommandType.Text = AppResources.minechat_general_commandtype;

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
                save.Text = "Save";
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
            if(entryCommand.Text == null || entryCommand.Text.Trim()  == string.Empty)
            {
                ShowMessage("Command cannot be blank");
                return;
            }

            MineChatAPI.Command c = (MineChatAPI.Command)this.BindingContext;
            c.Type = (MineChatAPI.CommandType)pickerType.SelectedIndex;

            save = true;
            Close();
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

        private void ShowMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Add Command Failed", message, "Ok");
            });
        }

    }
}
