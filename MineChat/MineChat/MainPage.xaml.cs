using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineChatAPI;
using Xamarin.Forms;
using System.Diagnostics;

namespace MineChat
{
    public partial class MainPage : MasterDetailPage
    {
        MasterPage _masterPage;


        public MainPage()
        {
            _masterPage = new MasterPage();
            Master = _masterPage;
            this.IsPresentedChanged += (sender, e) => 
            {
				var keyboard = DependencyService.Get<IKeyboardNotifications>();
				if (keyboard != null)
				{
					keyboard.HideKeyboard();
				}                
            };
            this.MasterBehavior = MasterBehavior.Popover;
            
            MasterPageItem pageItem = null;

            // See if there are any accounts, if so, then take them to the servers page
            if (Global.CurrentSettings.Accounts.Count == 0)
            {
                pageItem = _masterPage.masterPageItems.Find(m => m.TargetType == typeof(AccountsPage));                
            }
            else
            {
                pageItem = _masterPage.masterPageItems.Find(m => m.TargetType == typeof(ServersPage));
            }

            _masterPage.ListView.ItemSelected += OnItemSelected;
            SelectItem(pageItem);
        }

        private ServersPage GetServersPage()
        {
            MasterPageItem pageItem = _masterPage.masterPageItems.Find(m => m.TargetType == typeof(ServersPage));
            if (pageItem.CurrentInstance == null)
            {
                pageItem.CurrentInstance = new NavigationPage((Page)Activator.CreateInstance(typeof(ServersPage))) { BarBackgroundColor = Global.MainColor, BarTextColor = Color.White };
            }

            NavigationPage serversPage = (NavigationPage)pageItem.CurrentInstance;
            Detail = serversPage;
            ServersPage ct = (ServersPage)serversPage.CurrentPage;
            _masterPage.ListView.SelectedItem = null;
            IsPresented = false;

            return ct;

        }

        private ChatPage GetChatPage()
        {
            try
            {
                MasterPageItem pageItem = _masterPage.masterPageItems.Find(m => m.TargetType == typeof(ChatPage));
                if (pageItem.CurrentInstance == null)
                {
                    pageItem.CurrentInstance = new NavigationPage((Page)Activator.CreateInstance(typeof(ChatPage))) { BarBackgroundColor = Global.MainColor, BarTextColor = Color.White };
                }

                NavigationPage chatPage = (NavigationPage)pageItem.CurrentInstance;
                Detail = chatPage;
                ChatPage ct = (ChatPage)chatPage.CurrentPage;
                _masterPage.ListView.SelectedItem = null;
                IsPresented = false;

                return ct;
            }
            catch(Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }        

        public void ServerConnect(Server server)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ChatPage cp = GetChatPage();
                cp.ServerConnect(server);
            });
        }

        public void ShowServers()
        {   
            Device.BeginInvokeOnMainThread(() =>
            {
                MasterPageItem pageItem = _masterPage.masterPageItems.Find(m => m.TargetType == typeof(ServersPage));
                GetServersPage();
                SelectItem(pageItem);
            });
        }

        public void ServerDisconnect()
        {
            ChatPage cp = GetChatPage();
            cp.ServerDisconnect();
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            SelectItem(item);
        }

         
        private void SelectItem(MasterPageItem item)
        {            
            if (item != null)
            {
                try
                {
                    if (item.CurrentInstance == null)
                    {
                        Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType)) { BarBackgroundColor = Global.MainColor, BarTextColor = Color.White };                                        
                        item.CurrentInstance = Detail;
                        _masterPage.ListView.SelectedItem = null;
                    }
                    else
                    {
                        Detail = item.CurrentInstance;
                    }

                    IsPresented = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
