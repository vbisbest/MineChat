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
    public partial class AccountsPage : ContentPage
    {

        bool selecting = false;
        ObservableCollection<AccountWrapper> accounts = new ObservableCollection<AccountWrapper>();
        bool editing = false;

        public AccountsPage()
        {
            InitializeComponent();
            this.Title = AppResources.minechat_general_accounts;
            labelDescription.Text = AppResources.minechat_screens_account;
            SetupPage();
        }

        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Global.CurrentSettings.Accounts.Count == 0 && !editing)
            {
                UserDialogs.Instance.AlertAsync(AppResources.minechat_general_getstarted).ContinueWith((Task state) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        AddAccount();
                    });
                });                
            }
        }
        

        private void SetupPage()
        {
            try
            {
                listView.HasUnevenRows = false;                
                listView.ItemSelected += listView_ItemSelected;

                ToolbarItem addAccount = new ToolbarItem();
                addAccount.Clicked += AddAccount_Clicked;
                addAccount.Icon = "add.png";
                addAccount.Text = AppResources.minechat_general_ok;
                labelDescription.Text = AppResources.minechat_screens_account;
                this.ToolbarItems.Add(addAccount);

                foreach (Account currentAccount in Global.CurrentSettings.Accounts)
                {
                    AccountWrapper aw = new AccountWrapper();
                    aw.Account = currentAccount;
                    accounts.Add(aw);
                }

                // Set the list view source
                listView.ItemsSource = accounts;
                
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // An account was selected, show the actions
            if (e.SelectedItem == null || selecting)
            {
                return;
            }

            selecting = true;

            AccountWrapper selectedAccount = (AccountWrapper)e.SelectedItem;
            DisplayActions(selectedAccount);
        }

        private async void DisplayActions(AccountWrapper account)
        {           
            await DisplayActionSheet(AppResources.minechat_general_accounts, AppResources.minechat_general_cancel, null, AppResources.minechat_general_select, AppResources.minechat_general_edit, AppResources.minechat_general_delete).ContinueWith(getTask =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    selecting = false;

                    if(getTask.Result == AppResources.minechat_general_select)
                    {
                        SelectAccount(account);
                        SaveAccounts();
                    }
                    else if (getTask.Result == AppResources.minechat_general_edit)
                    {
                        ShowEditAccount(account);
                    }
                    else if (getTask.Result == AppResources.minechat_general_delete)
                    {
                        accounts.Remove(account);
                        SaveAccounts();
                    }

                    listView.SelectedItem = null;
                });
            });
        }

        private void AddAccount_Clicked(object sender, EventArgs e)
        {
            AddAccount();
        }

        private void SaveAccount(AccountWrapper account)
        {

            if(account.IsOffline)
            {
                account.PlayerName = account.UserName;
            }


            Device.BeginInvokeOnMainThread(() =>
            {
                if (!accounts.Contains(account))
                {
                    // Add account
                    accounts.Add(account);
                }
                account.Refresh();

                SaveAccounts();

                UserDialogs.Instance.HideLoading();

                if (Global.CurrentSettings.Accounts.Count == 1)
                {
                    UserDialogs.Instance.AlertAsync("Account added. You can now connect and add servers from the Servers page.").ContinueWith((Task state) =>
                    {
                        MainPage mp = (MainPage)App.Current.MainPage;
                        mp.ShowServers();
                    });
                }

            });
        }

        private async void ShowEditAccount(AccountWrapper account)
        {
            editing = true;

            AccountAddPage accountAdd = new AccountAddPage();
            accountAdd.BindingContext = account;
            
            accountAdd.Disappearing += delegate
            {
                // Came back from editing now save
                if (accountAdd.Save)
                {
                    SaveAccount(account);
                }
            };

            // Push the dialog
            await Navigation.PushAsync(accountAdd);           
        }

        private void AddAccount()
        {
            if(Product.IsLite)
            {
                if(Global.CurrentSettings.Accounts.Count >= 1)
                {
                    ShowMessage(AppResources.minechat_settings_accountslite);
                    return;
                }
            }

            AccountWrapper account = new AccountWrapper();
            ShowEditAccount(account);            
        }

        private void SelectAccount(AccountWrapper selectedAccount)
        {
            selectedAccount.Selected = true;
            
            // Push the accounts to the real settings
            foreach (AccountWrapper currentAccount in accounts)
            {
                if (currentAccount != selectedAccount)
                {
                    currentAccount.Selected = false;
                }
                currentAccount.Refresh();                
            }
        }

        private void SaveAccounts()
        {            
            Global.CurrentSettings.Accounts.Clear();

            AccountWrapper selectedAccount = accounts.FirstOrDefault(a => a.Selected == true);

            if(accounts.Count > 0 && selectedAccount == null)
            {
                // No default account, select the first one
                SelectAccount(accounts.FirstOrDefault());
            }

            // Push the accounts to the real settings
            foreach(AccountWrapper currentAccount in accounts)
            {
                Global.CurrentSettings.Accounts.Add(currentAccount.Account);
            }

            Global.SaveSettings();
            editing = false;
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
