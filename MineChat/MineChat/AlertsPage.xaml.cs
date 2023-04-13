using System;
using System.Collections.Generic;
using System.Linq;
using MineChatAPI;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections;
using Xamarin.Forms;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MineChat.Languages;

namespace MineChat
{
    public partial class AlertsPage : ContentPage
    {
        ObservableCollection<AlertWrapper> collection = new ObservableCollection<AlertWrapper>();

        public AlertsPage()
        {
            InitializeComponent();
            this.Title = AppResources.minechat_general_alerts;
            labelDescription.Text = AppResources.minechat_screens_alerts;

            SetupPage();
        }

        private void SetupPage()
        {
            try
            {
                ToolbarItem add = new ToolbarItem();
                add.Clicked += Add_Clicked;
                add.Icon = "add.png";
                add.Text = AppResources.minechat_settings_add;

                // Set the list view source
                SetSource();

                this.ToolbarItems.Add(add);

                listView.SeparatorColor = Color.Gray;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void SetSource()
        {
            listView.ItemsSource = null;
            collection.Clear();

            foreach (MineChatAPI.AlertWord current in Global.CurrentSettings.Alerts)
            {
                AlertWrapper wrapper = new AlertWrapper();
                wrapper.AlertWord = current;
                collection.Add(wrapper);
            }

            listView.ItemsSource = collection;
        }

        private void Add_Clicked(object sender, EventArgs e)
        {
            AddAlert();
        }

        private void SaveCommand(AlertWrapper alert)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!collection.Contains(alert))
                {
                    // Add account
                    collection.Add(alert);
                }

                Save();
            });
        }

        private async void ShowEditCommand(MineChatAPI.AlertWord alert, bool adding)
        {
            AlertAddPage addPage = new AlertAddPage();
            addPage.BindingContext = alert;

            addPage.Disappearing += delegate
            {
                // Came back from editing now save
                if (addPage.Save)
                {
                    if (adding)
                    {
                        Global.CurrentSettings.Alerts.Add(alert);
                    }

                    SetSource();
                    Global.SaveSettings();

                }
            };

            // Push the dialog
            await Navigation.PushAsync(addPage);
        }

        private void AddAlert()
        {
            if (Product.IsLite)
            {
                if (Global.CurrentSettings.Alerts.Count >= 2)
                {
                    ShowMessage(AppResources.minechat_settings_alertslite);
                    return;
                }
            }

            MineChatAPI.AlertWord alert = new MineChatAPI.AlertWord();
            ShowEditCommand(alert, true);
        }

        public void OnEdit(object sender, EventArgs e)
        {
            AlertWrapper wrapper = (sender as Xamarin.Forms.MenuItem).BindingContext as AlertWrapper;
            MineChatAPI.AlertWord edit = Global.CurrentSettings.Alerts.Find(c => c.Word == wrapper.Word);
            ShowEditCommand(edit, false);
        }

        public void OnDelete(object sender, EventArgs e)
        {
            try
            {
                AlertWrapper wrapper = (sender as Xamarin.Forms.MenuItem).BindingContext as AlertWrapper;
                Global.CurrentSettings.Alerts.RemoveAll(s => s.Word == wrapper.Word);
                Global.SaveSettings();
                SetSource();
            }
            catch
            {
                Debug.WriteLine("Remove failed");
            }
        }

        private void Save()
        {
            Global.CurrentSettings.Alerts.Clear();

            // Push the accounts to the real settings
            foreach (AlertWrapper current in collection)
            {
                current.Refresh();
                Global.CurrentSettings.Alerts.Add(current.AlertWord);
            }

            Global.SaveSettings();
        }

        private void ShowMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("MineChat", message, AppResources.minechat_general_ok);
            });
        }
    }
}
