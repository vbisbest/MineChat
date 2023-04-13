using System;
using System.Collections.Generic;
using System.Linq;
using MineChatAPI;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections;
using Xamarin.Forms;
using MineChat.Languages;
using Acr.UserDialogs;
using System.Threading.Tasks;
using System.Reflection;

namespace MineChat
{
    public partial class ServersPage : ContentPage
    {
        ObservableCollection<ServerWrapper> allServers = new ObservableCollection<ServerWrapper>();
        IEnumerable sorted = null;
        ServerInfo2 serverInfo = new ServerInfo2();

        //IDNS dns = DependencyService.Get<IDNS>();
        DNS dns = new DNS();

        MojangAPIV2 api = new MojangAPIV2();
        ObservableCollection<Group> groupedItems = new ObservableCollection<Group>();

        bool editing = false;
        bool connecting = false;
        bool initialized = false;

        //DNS dnsClient = new DNS();

        public ServersPage()
        {
            InitializeComponent();
            this.Title = AppResources.minechat_general_servers;
            SetupPage();
        }
        
        protected override void OnAppearing()
        {

            connecting = false;

            if (!editing)
            {
                if (!initialized || (DateTime.Now - Global.CurrentSettings.LastFeaturedServerUpdate).TotalHours > 4 || Global.FeaturedServersChanged)
                {
                    UserDialogs.Instance.ShowLoading();
                    GetServers();
                }
            }
            else
            {
				editing = false;
                UserDialogs.Instance.HideLoading();
            }

            base.OnAppearing();

        }

        private void ClearServers()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                groupedItems.Clear();
            });
        }

        private void GetServers()
        {
            try
            {
                //ShowMessage("GetServers");

                initialized = true;
                Global.FeaturedServersChanged = false;

                if (Global.MineFriendsConnect == string.Empty)
                {
                    ClearServers();
                    SetupGroups();

                    foreach (Server server in Global.CurrentSettings.Servers)
                    {
                        ServerWrapper sw = new ServerWrapper(server);
                        AddGroupedServer(sw);
                    }


                    if (Global.CurrentSettings.ShowFeaturedServers)
                    {
                        GetPromotedServers();
                    }

                    if(groupedItems.Count == 0)
                    {
                        UserDialogs.Instance.HideLoading();
                    }
                    

                    GetRealmsServers();

                }
                else
                {
                    // Being launched from Minefriends
                    //ServerWrapper server = new ServerWrapper(new Server());
                    //server.FullAddress = Global.MineFriendsConnect;
                    //allServers.Clear();
                    //server.Info = AppResources.minechat_general_polling;
                    //server.ServerName = Global.MineFriendsConnect;
                    //this.allServers.Add(server);
                    //dns.GetRealIPAndPort((Server)server);
                }
            }
            catch(Exception ex)
            {
                ShowMessage("Error: " + ex.GetBaseException().Message);
            }
        }
        

        private void SetupPage()
        {
            try
            {
                serverInfo.ServerInfoResult += ServerInfo_ServerInfoResult;
                serverInfo.ServerInfoError += ServerInfo_ServerInfoError;
             
                // Add toolbar items
                ToolbarItem addServer = new ToolbarItem();
                addServer.Clicked += AddServer_Clicked;
                addServer.Text = AppResources.minechat_settings_add;
                addServer.Icon = "add.png";

                ToolbarItem refreshServers = new ToolbarItem();
                refreshServers.Clicked += RefreshServers_Clicked;
                refreshServers.Text = AppResources.minechat_general_refresh;
                refreshServers.Icon = "refresh.png";

                this.ToolbarItems.Add(refreshServers);
                this.ToolbarItems.Add(addServer);

				listView.HasUnevenRows = true;
                listView.SeparatorColor = Color.Transparent;

                listView.ItemSelected += listView_ItemSelected;
                listView.IsGroupingEnabled = true;

                //dnsClient.OnDNSLookupComplete += Dns_OnDNSLookupComplete;
                
                connecting = false;
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.GetBaseException().Message);
                Debug.WriteLine(ex.Message);
            }
        }

        private void SetupGroups()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                listView.ItemsSource = null;

                Group group3 = new Group(ServerTypeEnum.User, AppResources.minechat_general_user);
                groupedItems.Add(group3);

                if (Global.CurrentSettings.ShowFeaturedServers)
                {
                    Group group1 = new Group(ServerTypeEnum.Featured, AppResources.minechat_general_featured);
                    groupedItems.Add(group1);

                    Group group2 = new Group(ServerTypeEnum.Promoted, AppResources.minechat_general_promoted);
                    groupedItems.Add(group2);
                }



                Group group4 = new Group(ServerTypeEnum.Realms, "Realms");
                groupedItems.Add(group4);

                listView.ItemsSource = groupedItems;
            });
        }

        private void ShowRate()
        {
            if(Global.CurrentSettings.AppRated)
            {
                return;
            }
            else
            {
                var rate = DependencyService.Get<IRate>();
                if (rate != null)
                {
                    rate.DoRate();
                    Global.CurrentSettings.AppRated = true;
                    Global.SaveSettings();
                }
            }
        }

        private void ShowFacebook()
        {
            if (Global.CurrentSettings.FacebookAsked)
            {
                return;
            }
            
            Acr.UserDialogs.ConfirmConfig cc = new Acr.UserDialogs.ConfirmConfig();
            cc.Message = "Stay up to date with the latest MineChat news and info by liking our Facebook page.  Would you like to do this now?";
            cc.OkText = "Yes";
            cc.CancelText = "No";
            cc.OnAction += delegate (bool result)
            {
                if (result == true)
                {
                    var d = DependencyService.Get<IDevice>();
                    if (d != null)
                    {
                        d.OpenFacebook();
                    }

                }
            };

            Device.BeginInvokeOnMainThread(delegate
            {
                Acr.UserDialogs.UserDialogs.Instance.Confirm(cc);
                Global.CurrentSettings.FacebookAsked = true;
                Global.SaveSettings();
            });

        }

        private void GetPromotedServers()
        {
            //ShowMessage("GetPromotedServers");

            if (Global.CurrentSettings.ShowFeaturedServers && (Global.CurrentPromotedServers.Servers.Count == 0 || (DateTime.Now - Global.CurrentSettings.LastFeaturedServerUpdate).TotalHours > 4))
            {
                FeaturedServers.GetFeaturedServersAsync().ContinueWith(task =>
                {
                    if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        Global.CurrentSettings.LastFeaturedServerUpdate = DateTime.Now;
                        Global.SaveSettings();

                        foreach (Server currentServer in task.Result)
                        {
                            ServerWrapper sw = new ServerWrapper(currentServer);
                            AddGroupedServer(sw);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error getting promoted servers");
                    }
                });
            }
        }

        private void GetRealmsServers()
        {            
            Account account = Global.CurrentSettings.GetSelectedAccount();

            if(account.IsOffline)
            {
                return;
            }

            try
            {
                MinecraftAPI.AuthenticateAccount(Global.GetSelectedAccount(), false);
                Global.SaveSettings();

                // Get realms servers
                MojangAPIV2.GetRealmsAvailable(account.AccessToken, account.ProfileID, account.PlayerName).ContinueWith(task =>
                {
                    if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        if (task.Result)
                        {
                            Debug.WriteLine("Realms available");
                            MojangAPIV2.GetRealmsWorlds(account.AccessToken, account.ProfileID, account.PlayerName).ContinueWith(worldTask =>
                            {
                                if (worldTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                                {
                                    foreach (RealmsServer currentServer in worldTask.Result.servers)
                                    {
                                        Server rServer = new Server();
                                        rServer.ServerName = currentServer.name;
                                        rServer.MOTD = currentServer.motd;
                                        rServer.ServerType = ServerTypeEnum.Realms;
                                        rServer.Status = ServerStatus.OK;
                                        rServer.ProtocolVersion = ProtocolVersion.V4;
                                        rServer.Protocol = Global.CurrentRealmsProtocol;
                                        rServer.PromotedServerID = currentServer.id;
                                        rServer.ID = currentServer.id.ToString();

                                        int players = 0;
                                        if (currentServer.players != null)
                                        {
                                            players = currentServer.players.players.Count;
                                        }
                                        rServer.Info = string.Format("{0}/{1}", players, currentServer.maxPlayers);

                                        if (currentServer.minigameImage != null)
                                        {
                                            rServer.FavIcon = Convert.FromBase64String(currentServer.minigameImage);
                                        }
                                        else
                                        {
                                            rServer.FavIcon = null;
                                        }

                                        var gs = groupedItems.FirstOrDefault(s => s.GroupType == ServerTypeEnum.Realms);
                                        var test = gs.FirstOrDefault(s => s.ID == rServer.ID);
                                        if (test == null)
                                        {
                                            ServerWrapper sw = new ServerWrapper(rServer);
                                            AddGroupedServer(sw);
                                        }
                                    }
                                }
                                else
                                {
                                    throw task.Exception;
                                }
                            });
                        }
                    }
                    else
                    {
                        throw task.Exception;
                    }
                });
            }
            catch (Exception ex)
            {
                ShowMessage("Failed to retrieve Realms servers. " + ex.GetBaseException().Message);
            }
        }

        /*
        private void Dns_OnDNSLookupComplete(object sender, EventArgs e)
        {
            try
            {
                OnDNSLookupCompleteEventArgs args = (OnDNSLookupCompleteEventArgs)e;
                ServerWrapper server = (ServerWrapper) args.Server;

                if (args.Success)
                {
                    try
                    {
                        Debug.WriteLine("DNS Complete: " + server.Address + ":" + server.RealIP);

                        if (server.ServerType != ServerTypeEnum.Realms)
                        {
                            serverInfo.GetServerInfo(args.Server, false, false, Device.OS == TargetPlatform.Windows);
                        }                        
                        else
                        {
                            RefreshServer(server);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                else
                {                    
                    RefreshServer(server);
                }               
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.GetBaseException().Message);
                Debug.WriteLine(ex.Message);
            }
        }
        */

        private void ServerInfo_ServerInfoError(Server server, string message)
        {
            // Server error
            ServerWrapper sw = (ServerWrapper)server;
            RefreshServer(sw);
        }

        private void ServerInfo_ServerInfoResult(Server server)
        {
            // Server is ok
            //Debug.WriteLine("Got: " + server.RealIP);
            ServerWrapper sw = (ServerWrapper)server;
            RefreshServer(sw);

            if(Global.MineFriendsConnect!=string.Empty)
            {
                Global.MineFriendsConnect = string.Empty;

                MainPage mp = (MainPage)App.Current.MainPage;
                mp.ServerConnect(server);
            }
        }


        private void RefreshServers_Clicked(object sender, EventArgs e)
        {
            RefreshServerInfo();
        }

        private void AddServer_Clicked(object sender, EventArgs e)
        {
            AddServer();            
        }

        private void AddServer()
        {
            if (Product.IsLite)
            {
                if (Global.CurrentSettings.Servers.FindAll(s => s.ServerType == ServerTypeEnum.User).Count >= 2)
                {
                    ShowMessage(AppResources.minechat_settings_serverslite);
                    return;
                }
            }

            Server server = new Server();
            ServerWrapper sw = new ServerWrapper(server);
            ShowEditServer(sw, true);
        }

        private async void ShowEditServer(ServerWrapper server, bool adding)
        {
            ServerAddPage serverAdd = new ServerAddPage();
            serverAdd.BindingContext = server;

            serverAdd.Disappearing += delegate
            {
                ServerWrapper updateServer = null;
                try
                {
                    // Came back from editing now save
                    if (serverAdd.Save)
                    {
                        if (adding)
                        {
                            Global.CurrentSettings.Servers.Add(server.Clone());
                            AddGroupedServer(server);
                        }
                        else
                        {
                            Server st = Global.CurrentSettings.Servers.Find(s => s.ID == server.ID);                            
                            st.FullAddress = server.FullAddress;
                            st.ServerName = server.ServerName;
                            st.UseOffline = server.UseOffline;
                            st.ServerVersion = server.ServerVersion;
                            RefreshServerInfo(server);
                        }

                        Global.SaveSettings();
                        
                    }

                }
                catch(Exception ex)
                {
                    var analytics = DependencyService.Get<IAnalytics>();
                    if (analytics != null)
                    {
                        analytics.LogError("Could not add server: " + ex.Message);
                    }
                    ShowMessage("Could not add server.  Please email support if you need help adding servers.");
                }
            };

            // Push the dialog
            editing = true;
            await Navigation.PushAsync(serverAdd);
        }

        private void AddGroupedServer(ServerWrapper server)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {                    
                    var gs = groupedItems.First(g => g.GroupType == server.ServerType);                    
                    gs.Add(server);
                    

                    if (server.ServerType != ServerTypeEnum.Realms)
                    {
                        RefreshServerInfo(server);
                    }
                    else
                    {
                        RefreshServer(server);
                    }
                }
                catch(Exception ex)
                {
                    ShowMessage("Error: " + ex.GetBaseException().Message);
                    Debug.WriteLine(ex.Message);
                }
            });


        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            //var analytics = DependencyService.Get<IAnalytics>();
            //analytics.ConnectToServer(null);

            //return;

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    Server selectedServer = (Server)listView.SelectedItem;

                    listView.SelectedItem = null;

                    if (selectedServer == null || connecting)
                    {
                        return;
                    }

                    if (selectedServer.Status == ServerStatus.Polling)
                    {
                        ShowMessage("Wait until polling is complete");
                        return;
                    }
                    else if (selectedServer.Status == ServerStatus.Error)
                    {
                        ShowMessage("Unable to connect to server");
                        return;
                    }

                    connecting = true;

                    // Get realms address
                    if (selectedServer.ServerType == ServerTypeEnum.Realms)
                    {
                        UserDialogs.Instance.ShowLoading("Connecting");

                        Account account = Global.CurrentSettings.GetSelectedAccount();
                        try
                        {
                            MojangAPIV2.GetRealmsServerAddress(account.AccessToken, account.ProfileID, account.PlayerName, selectedServer.PromotedServerID).ContinueWith(task =>
                            {
                                if(task.IsFaulted)
                                {
                                    ShowMessage(task.Exception.GetBaseException().Message);
                                    connecting = false;
                                    return;
                                }

                                string address = task.Result;
                                selectedServer.FullAddress = address;

                                dns.DoLookup(selectedServer).ContinueWith(resolveTask =>
                                {
                                    if (resolveTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            UserDialogs.Instance.HideLoading();
                                            MainPage rmp = (MainPage)App.Current.MainPage;
                                            rmp.ServerConnect(resolveTask.Result);
                                        });
                                    }
                                });
                            });

                        }
                        catch (Exception ex)
                        {
                            ShowMessage(ex.Message);
                        }
                        return;
                    }

                    try
                    {
                        if (selectedServer.ServerType == ServerTypeEnum.Featured || selectedServer.ServerType == ServerTypeEnum.Promoted)
                        {
                            FeaturedServers featuredServers = new FeaturedServers();
                            featuredServers.UpdateFeaturedServers(selectedServer.PromotedServerID);
                        }

                    }
                    catch (Exception ex)
                    {
                        // Ignore
                        Debug.WriteLine(ex.Message);
                        //ShowMessage("Error: " + ex.Message);
                    }

                    MainPage mp = (MainPage)App.Current.MainPage;
                    mp.ServerConnect(selectedServer);
                }
                catch(Exception ex)
                {
                    // ignore
                    Console.WriteLine("Error: " + ex.Message);
                }
            });
        }

        private void RefreshServerInfo(ServerWrapper server)
        {
            if(server.ServerType == ServerTypeEnum.Realms)
            {
                return;
            }

            Debug.WriteLine("Refresh: " + server.ServerName);

            server.Status = ServerStatus.Polling;
            server.Info = AppResources.minechat_general_polling;
            RefreshServer(server);

            dns.DoLookup(server).ContinueWith(task =>
            {
                if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                    serverInfo.GetServerInfo(task.Result, false, false, Device.OS == TargetPlatform.iOS);
                }
            });
        }

        private void D_OnDNSLookupComplete(OnDNSLookupCompleteEventArgs e)
        {
            Console.WriteLine(e.Server.RealIP);
        }

        private void RefreshServerInfo()
        {
            foreach (Group servers in groupedItems)
            {
                foreach (ServerWrapper server in servers)
                {
                    RefreshServerInfo(server);
                }
            }

            GetRealmsServers();
        }

        public void OnEdit(object sender, EventArgs e)
        {
            ServerWrapper sw = (sender as Xamarin.Forms.MenuItem).BindingContext as ServerWrapper;

            if(sw.ServerType!= ServerTypeEnum.User)
            {
                ShowMessage(AppResources.minechat_general_noeditpromoted);
                return;
            }

            //Server editServer = Global.CurrentSettings.Servers.Find(s => s.ID == sw.ID);
            ShowEditServer(sw, false);
        }

        public void OnDelete(object sender, EventArgs e)
        {
            try
            {
                ServerWrapper server = (sender as Xamarin.Forms.MenuItem).BindingContext as ServerWrapper;

                if (server.ServerType != ServerTypeEnum.User)
                {
                    ShowMessage(AppResources.minechat_general_noeditpromoted);
                    return;
                }

                Global.CurrentSettings.Servers.RemoveAll(s => s.ID == server.ID);
                Global.SaveSettings();

                var gs = groupedItems.First(g => g.GroupType == server.ServerType);
                gs.Remove(server);
            }
            catch
            {
                Debug.WriteLine("Remove faialed");
            }
        }

        private void RefreshServer(ServerWrapper sw)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();
                //Debug.WriteLine(groupedItems.Count);

                if(sw.Status == ServerStatus.Error)
                {
                    sw.Info = AppResources.minechat_general_error;
                }
                sw.Refresh();
            });
        }

        private void ShowMessage(string message)
        {

            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();
                DisplayAlert("MineChat", message, AppResources.minechat_general_ok);
            });
        }
    }

    public class Group : ObservableCollection<ServerWrapper>
	{
        private string name;

		public string Name
        {
            get
            {
                return name;
            }
            
            set
            {
                this.name = value;
            }
        }

        public ServerTypeEnum GroupType { get; private set; }

		public Group(ServerTypeEnum groupType, string Name)
		{
			this.Name = Name;
            this.GroupType = groupType;
		}

		// Whatever other properties
	}
}
