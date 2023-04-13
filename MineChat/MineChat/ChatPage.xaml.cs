using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using MineChatAPI;
using System.Diagnostics;
using Acr.UserDialogs;
using MineChat.Languages;
using System.Linq;
using System.Collections;

namespace MineChat
{
    public partial class ChatPage : ContentPage
    {
        ActionBarMessage actionBarMessage = new ActionBarMessage();

        private bool bannerSent;
        private Server currentServer;
        private bool forceConnect;
        private bool sentSpawn;
        private bool profanityFilterEnabled;
        private string lastMessageSent = string.Empty;
        private double lastHeight = 0;
        private List<string> chatHistory = new List<string>();
        private int chatHistoryIndex = 0;
        private MojangAPI mojangAPI = new MojangAPI();
        private bool respawnShown = false;

        private IKeyboardNotifications keyboard;

        Dictionary<string, DateTime> lastLogin = new Dictionary<string, DateTime>();

        bool playersExpanded = false;
        private Server selectedServer = null;

        ToolbarItem toolItemDisconnect;
        ToolbarItem toolItemTab;

        public ChatPage()
        {
            InitializeComponent();
            this.Title = AppResources.minechat_general_chat;
            SetupPage();
        }

        private void SetupPage()
        {
            listViewChat.HasUnevenRows = true;

            Global.OnlinePlayers = new ObservableCollection<Player>();
            listViewPlayers.ItemsSource = Global.OnlinePlayers;
            listViewPlayers.HasUnevenRows = true;
            listViewPlayers.RowHeight = 50;

            keyboard = DependencyService.Get<IKeyboardNotifications>();
            if (keyboard != null)
            {
                keyboard.HookKeyboard(shiftableStack, this);
                keyboard.OnKeyboardShown += Keyboard_OnKeyboardShown;
            }

            listViewChat.ItemSelected += listViewChat_ItemSelected;
            listViewChat.ItemTapped += ListViewChat_ItemTapped;
            listViewPlayers.ItemTapped += ListViewPlayers_ItemTapped;

            lableActionBar.BindingContext = actionBarMessage;
            entryChat.OnSendChat += EntryChat_Completed;
            entryChat.OnHistoryBackward += EntryChat_OnHistoryBackward;
            entryChat.OnHistoryForward += EntryChat_OnHistoryForward;
            entryChat.OnTapped += EntryChat_OnTapped;
            toolItemTab = new ToolbarItem();
            toolItemTab.Clicked += ToolItemTab_Clicked;
            toolItemTab.Icon = "tab.png";
            toolItemTab.Text = "Tab";

            toolItemDisconnect = new ToolbarItem();
            toolItemDisconnect.Clicked += ToolItemDisconnect_Clicked;
            toolItemDisconnect.Icon = "disconnect.png";
            toolItemDisconnect.Text = AppResources.minechat_servers_disconnect;
            toolItemDisconnect.IsDestructive = true;

            Tab(false);

            listViewChat.SizeChanged += ListViewChat_SizeChanged;
            //entryChat.IsEnabled = false;
            mojangAPI.PlayerInfoCompleted += MojangAPI_PlayerInfoCompleted;

            //string s = "{\"translate\":\"death.fell.accident.generic\",\"with\":[{\"insertion\":\"toodumb2live2\",\"clickEvent\":{\"action\":\"suggest_command\",\"value\":\"/msg toodumb2live2 \"}}]}" ;
            //string s = "{\"translate\":\"death.fell.accident.generic\",\"with\":[{\"insertion\":\"HogwartsDragon\",\"clickEvent\":{\"aFFction\":\"suggest_command\",\"value\":\"/msg HogwartsDragon \"},\"hoverEvent\":{\"action\":\"show_entity\",\"value\":{\"text\":\"{name:\"HogwartsDragon\",id:\"ff0920b1-ccdc-454c-8034-859c18fcd362\"}\"}},\"text\":\"§8u003c§7u003c§2HogwartsDragon§7u003e§8u003e\"}]}";
            //string p = MineChatChat.ChatProcessor.MinecraftStringToAttributedStringJson(s);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (height != lastHeight)
            {

                if (entryChat.IsFocused)
                {
                    if (Device.OS == TargetPlatform.iOS)
                    {
                        DismissKeyboard();
                    }
                }

                entryChat.ConfigureEntry();
                lastHeight = height;
            }

            base.OnSizeAllocated(width, height);
        }


        void OnTapGestureRecognizerTapped(object sender, EventArgs args)
        {
            SendRespawn();
        }


        private void ListViewChat_SizeChanged(object sender, EventArgs e)
        {
            //ScrollToBottom();
        }

        private void ListViewPlayers_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            listViewPlayers.SelectedItem = null;
            Player player = (Player)e.Item;
            ShowPlayerCommands(player);
        }

        private void Keyboard_OnKeyboardShown(object sender, EventArgs e)
        {
            entryChat.Focus();
            ScrollToBottom();
        }

        private void listViewChat_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            listViewChat.SelectedItem = null;

            if (entryChat.IsFocused)
            {
                DismissKeyboard();
                return;
            }

            ItemTappedEventArgs args = new ItemTappedEventArgs(null, e.SelectedItem);
            ListViewChat_ItemTapped(sender, args);
        }

        private void DismissKeyboard()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                entryChat.HideKeyboard();
                entryChat.Unfocus();
            });
        }

        private void ListViewChat_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ChatMessage chatMessage = (ChatMessage)e.Item;

            var cfg = new ActionSheetConfig();
            cfg.SetTitle("Chat");
            cfg.Add(AppResources.minechat_general_copy, () =>
            {
                var device = DependencyService.Get<IDevice>();
                device.CopyToClipboard(chatMessage.CleanMessageKeepCase);
            });

            string link = chatMessage.GetLink;


            if (link != string.Empty)
            {
                cfg.Add(AppResources.chat_link_open + " " + link, () =>
                {
                    var device = DependencyService.Get<IDevice>();

                    try
                    {
                        device.OpenLink(link);
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Could not open link " + link);
                        Debug.WriteLine(ex.Message);
                    }
                });
            }

            cfg.SetCancel(AppResources.minechat_general_cancel);

            UserDialogs.Instance.ActionSheet(cfg);
            listViewChat.SelectedItem = null;
        }

        private void ShowPlayerCommands(Player player)
        {
            List<MineChatAPI.Command> commands = Global.CurrentSettings.Commands.FindAll(c => c.Type == CommandType.Player);
            IEnumerable sorted = commands.OrderBy(c => c.CommandText);

            var cfg = new ActionSheetConfig();
            cfg.SetTitle(player.CleanName);

            foreach (MineChatAPI.Command currentCommand in sorted)
            {
                cfg.Add(currentCommand.CommandText, () => BuildCommand(player.CleanName, currentCommand.CommandText));
            }

            cfg.SetCancel(AppResources.minechat_general_cancel);

            UserDialogs.Instance.ActionSheet(cfg);

        }

        private void BuildCommand(string player, string command)
        {
            entryChat.Focus();
            entryChat.Text = command + " " + player + " ";
        }

        private void EntryChat_Completed(object sender, EventArgs e)
        {
            if (entryChat.Text == string.Empty)
            {
                return;
            }

            chatHistory.Insert(0, entryChat.Text);
            chatHistoryIndex = 0;

            SendChat(entryChat.Text);
        }

        private void UpdatePlayer(string name, bool online, string uuid)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Player player = new Player();

                player.Name = name;
                player.UUID = uuid;

                try
                {
                    if (online)
                    {
                        if(Global.CurrentPlayers.Contains(player.UUID))
                        {
                            //Debug.WriteLine("player already online");
                            return;
                        }
                        else
                        {
                            Global.CurrentPlayers.Add(player.UUID);
                        }


                        if (Global.SkinCache.ContainsKey(player.UUID))
                        {
                            //Debug.WriteLine("player online - use skin cache");
                            player.Skin = Global.SkinCache[player.UUID];
                            player.Refresh();
                            Global.OnlinePlayers.Add(player);
                        }
                        else
                        {
                            //Debug.WriteLine("player online - get skin");
                            mojangAPI.GetPlayerInfo(player.UUID, name);
                        }
                    }
                    else
                    {
                        if (Global.OnlinePlayers.Contains(player))
                        {
                            //Debug.WriteLine("Remove player");
                            Global.CurrentPlayers.Remove(player.UUID);
                            Global.OnlinePlayers.Remove(player);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }

        private void MojangAPI_PlayerInfoCompleted(PlayerInfo playerInfo)
        {            
            //if (playerInfo.textureObject != null)
            //{
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        byte[] skin = null;

                        // New account
                        if (playerInfo.textureObject != null)
                        {
                            var graphics = DependencyService.Get<IGraphics>();

                            byte[] cropped = graphics.CropImage(Convert.FromBase64String(playerInfo.textureObject.Textures.Skin.Bytes), 8, 8, 8, 8);
                            skin = graphics.ScaleImage(cropped, 60, 60);
                            UpdateSkinCache(playerInfo, skin);
                        }

                        Player player = new Player();

                        player.Name = playerInfo.name;
                        player.UUID = playerInfo.id;
                        player.Skin = skin;
                        player.Refresh();

                        Global.OnlinePlayers.Add(player);
                    }
                    catch(Exception ex)
                    {
                        UpdateSkinCache(playerInfo, null);
                    }
                });
            //}
            //else
            //{
            //    UpdateSkinCache(playerInfo, null);
            //}
        }

        private void UpdateSkinCache(PlayerInfo playerInfo, byte[] skin)
        {
            try
            {
                if (Global.SkinCache.ContainsKey(playerInfo.id))
                {
                    Global.SkinCache[playerInfo.id] = skin;
                }
                else
                {
                    Global.SkinCache.Add(playerInfo.id, skin);
                }
            }
            catch
            {
                // Ignore
            }
        }

        public void ServerDisconnect()
        {
            if (Global.IsConnected)
            {
                Global.MineChatAPI.Disconnect();
            }
        }

        public void ServerConnect(Server server)
        {
            selectedServer = server;

            if (Global.IsConnected)
            {
                forceConnect = true;
                ServerDisconnect();
            }
            else
            {
                EstablishConnection(server);
            }
        }

        public void EstablishConnection(Server server)
        {
            this.bannerSent = false;
            this.sentSpawn = false;
            this.profanityFilterEnabled = false;
            forceConnect = false;

            var analytics = DependencyService.Get<IAnalytics>();
            if (analytics != null)
            {
                analytics.ConnectToServer(server);
            }

            Global.MineChatAPI = new MinecraftAPI();
            Global.MineChatAPI.KeepAlive += new MinecraftAPI.KeepAliveHandler(this.MineChatAPI_KeepAlive);
            Global.MineChatAPI.ChatMessage += new MinecraftAPI.ChatMessageHandler(this.mineChatAPI_ChatMessage);
            Global.MineChatAPI.PlayerOnline += new MinecraftAPI.PlayerOnlineHandler(this.MineChatAPI_PlayerOnline);
            Global.MineChatAPI.ConnectionError += new MinecraftAPI.ConnectionErrrorHandler(this.MineChatAPI_ConnectionError);
            Global.MineChatAPI.Disconnected += new MinecraftAPI.DisconnectedHandler(this.MineChatAPI_Disconnected);
            Global.MineChatAPI.Connected += new MinecraftAPI.ConnectedHandler(this.MineChatAPI_Connected);
            Global.MineChatAPI.UpdateHealth += MineChatAPI_UpdateHealth;
            Global.MineChatAPI.Authenticated += MineChatAPI_Authenticated;

            chatHistory.Clear();
            chatHistoryIndex = 0;
            Global.SkinCache = new Dictionary<string, byte[]>();
            Global.CurrentPlayers = new List<string>();

            listViewChat.Init();

            this.AddChatMessage(AppResources.minechat_general_connectingto + " " + server.ServerName);

            Global.ServerConnectTime = DateTime.Now;
            currentServer = server;

            try
            {
                Global.MineChatAPI.EstablishConnection(server, Global.CurrentSettings);
                Title = server.ServerName;
            }
            catch (Exception ex)
            {
                AddChatMessage(ex.Message);
            }
        }

        private void SendRespawn()
        {

            if (labelHealth.Text != "0")
            {
                ShowMessage(AppResources.minechat_general_respawn);
                return;
            }

            if (respawnShown == false)
            {
                respawnShown = true;
                Acr.UserDialogs.ConfirmConfig cc = new Acr.UserDialogs.ConfirmConfig();
                cc.Message = "Your player has died! " + AppResources.minechat_general_respawnquestion;
                cc.OkText = AppResources.minechat_general_yes;
                cc.CancelText = AppResources.minechat_general_no;

                cc.OnAction += delegate (bool result)
                {
                    respawnShown = false;
                    if (result == true)
                    {
                        Global.MineChatAPI.Respawn();
                    }
                };

                Device.BeginInvokeOnMainThread(delegate
                {
                    Acr.UserDialogs.UserDialogs.Instance.Confirm(cc);
                });
            }
        }

        private void MineChatAPI_UpdateHealth(int health, int food)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                stackLayoutHealth.IsVisible = true;
                labelHealth.Text = health.ToString();
                labelFood.Text = food.ToString();

                if (health < 1)
                {
                    SendRespawn();
                }
            });
        }

        private void SendChat(string message)
        {
            //Debug.WriteLine(message);

            if (message == null || message == string.Empty)
            {
                return;
            }

            if (message.ToLower() == "/joinfriend")
            {
                ShowMessage(AppResources.minechat_general_joinfriend);
                return;
            }

            lastMessageSent = message;
            if (message.Length > 99 && currentServer.Protocol < 334)
            {
                message = message.Substring(0, 99);
            }
            else if (message.Length > 254)
            {
                message = message.Substring(0, 254);
            }

            Global.MineChatAPI.SendChat(message);
            entryChat.Text = string.Empty;

            /*
            if (profanityFilterEnabled && !message.StartsWith("/"))
            {
                string checkMessage = message.ToLower();
                // Check for profanity
                checkMessage = Global.RemoveDuplicates(checkMessage);
                checkMessage = checkMessage.Replace("$", "s");
                checkMessage = checkMessage.Replace("!", "i");
                checkMessage = checkMessage.Replace("0", "o");

                checkMessage = profanityRegex.Replace(checkMessage, "");
                checkMessage = checkMessage.Replace(" ", "");


                foreach (string currentProfanity in profanityList)
                {
                    if (checkMessage.Contains(currentProfanity))
                    {
                        //minecraftAPI.Disconnect();
                        ShowMessage("Warning! This server does not allow the use of profanity with MineChat");
                        return;
                    }
                }
            }
            */

        }

        public void MineChatAPI_Authenticated()
        {
            Global.SaveSettings();
        }

        private void MineChatAPI_ConnectionError(string message)
        {
            this.AddChatMessage(message);
            DisconnectCleanup();
        }

        private void MineChatAPI_Connected()
        {
            Global.IsConnected = true;
            Global.CurrentSettings.ConnectedCount++;
            Global.SaveSettings();



            Device.BeginInvokeOnMainThread(() =>
            {
                this.ToolbarItems.Add(toolItemDisconnect);
                this.ToolbarItems.Add(toolItemTab);

                Tab(true);

                entryChat.Placeholder = string.Empty;
                stackLayoutPlayers.IsVisible = true;
                entryChat.Focus();
            });

            ShowMessages();

        }

        private void DisconnectCleanup()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Global.IsConnected = false;
                stackLayoutHealth.IsVisible = false;
                Global.OnlinePlayers.Clear();
                actionBarMessage.FormattedActionBarText = null;

                Tab(false);

                this.ToolbarItems.Remove(toolItemDisconnect);
                this.ToolbarItems.Remove(toolItemTab);

                //entryChat.IsEnabled = false;
                entryChat.Placeholder = AppResources.minechat_servers_connect;

                if (forceConnect)
                {
                    EstablishConnection(selectedServer);
                }
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!Global.IsConnected)
            {
                entryChat.Placeholder = AppResources.minechat_servers_connect;
            }
            else
            {
                entryChat.Placeholder = "";
                ScrollToBottom();
            }
        }

        protected override void OnDisappearing()
        {
            entryChat.Detach();
            base.OnDisappearing();
        }

        private void MineChatAPI_Disconnected(string reason)
        {
            if (reason == string.Empty)
            {
                reason = AppResources.minechat_general_disconnected;
            }

            AddChatMessage(reason);
            DisconnectCleanup();
        }

        private void MineChatAPI_PlayerOnline(string name, bool online, string uuid)
        {
            if (name.StartsWith(" "))
            {
                return;
            }

            UpdatePlayer(name, online, uuid);
        }

        private void mineChatAPI_ChatMessage(string message, ChatType chatType)
        {
            Console.WriteLine(message);
            AddChatMessage(message, chatType);
        }

        private void AddChatMessage(string message)
        {
            AddChatMessage(message, ChatType.ChatBox);
        }


        private void AddChatMessage(string message, ChatType chatType)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (chatType == ChatType.ActionBar)
                    {
                        actionBarMessage.ActionBarText = message;
                    }
                    else
                    {
                        ChatMessage newMesasge = new ChatMessage();
                        newMesasge.ChatType = chatType;
                        newMesasge.Message = message;

                        if (newMesasge.CleanMessage == string.Empty)
                        {
                            return;
                        }

                        newMesasge.IsAlert = FindAlertWord(newMesasge);

                        listViewChat.AddMessage(newMesasge);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {

                }
            });

            //ScrollToBottom();

        }


        private void ScrollToBottom()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Debug.WriteLine("SCROLL TO BOTTOM");
                if (listViewChat != null)
                {
                    listViewChat.ScrollToBottom();
                }
            });
        }

        private void MineChatAPI_KeepAlive()
        {
            this.HandleKeepalive();
        }

        private void ConfirmDisconnect()
        {
            Acr.UserDialogs.ConfirmConfig cc = new Acr.UserDialogs.ConfirmConfig();
            cc.Message = "Are you sure you want to disconnect?";
            cc.OkText = AppResources.minechat_general_yes;
            cc.CancelText = AppResources.minechat_general_no;
            cc.OnAction += delegate (bool result)
            {
                if (result == true)
                {
                    ServerDisconnect();
                }
            };

            Device.BeginInvokeOnMainThread(delegate
            {
                Acr.UserDialogs.UserDialogs.Instance.Confirm(cc);
            });
        }

        private void ToolItemDisconnect_Clicked(object sender, EventArgs e)
        {
            DismissKeyboard();
            ConfirmDisconnect();
        }

        private void Tab(bool init)
        {
            if (!Global.IsConnected)
            {
                stackLayoutPlayers.IsVisible = false;
                playersExpanded = false;
                return;
            }

            if (playersExpanded || init)
            {
                if (Global.CurrentSettings.ShowHeads)
                {
                    stackLayoutPlayers.WidthRequest = 50;
                }
                else
                {
                    stackLayoutPlayers.WidthRequest = 0;
                }

                playersExpanded = false;
            }
            else
            {
                stackLayoutPlayers.WidthRequest = 300;
                playersExpanded = true;
            }
        }

        private void ToolItemTab_Clicked(object sender, EventArgs e)
        {
            Tab(false);
        }

        private void ShowMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("MineChat", message, AppResources.minechat_general_ok);
            });
        }

        private bool FindAlertWord(ChatMessage message)
        {
            bool isAlert = false;

            try
            {
                if (Global.CurrentSettings.Alerts.Count == 0)
                    return false;

                string str = message.CleanMessage;

                if (this.lastMessageSent != string.Empty && str.Contains(this.lastMessageSent.ToLower()))
                {
                    return false;
                }

                foreach (AlertWord alertWord in Global.CurrentSettings.Alerts)
                {
                    if (str.Contains(alertWord.Word.ToLower()))
                    {
                        var device = DependencyService.Get<IDevice>();

                        if (alertWord.Sound)
                        {
                            device.Sound();
                        }
                        if (alertWord.Vibrate)
                        {
                            device.Vibrate();
                        }


                        var notification = DependencyService.Get<INotification>();

                        if (notification != null)
                        {
                            notification.ShowNotification(str);
                        }

                        isAlert = true;
                        break;
                    }
                }


                return isAlert;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error finding alerts: " + ex.Message);
                return false;
            }
        }

        private void ShowMessages()
        {

            if (Global.CurrentSettings.Servers.Count == 0)
            {
                return;
            }

            FeaturedServers fs = new FeaturedServers();
            fs.MessagesReceived += Fs_MessagesReceived;
            fs.GetMessages((MineChatAPI.PlatformID)Product.PlatformId);
        }

        private void Fs_MessagesReceived(MessagesList messages, string error)
        {

            if (error != string.Empty)
            {
                ShowMessage("Error loading messages: " + error);
                return;
            }

            bool changed = false;

            foreach (MineChatAPI.Message currentMessage in messages.Messages)
            {
                if (Global.CurrentSettings.SeenMessages.Contains(currentMessage.MessageID))
                {
                    continue;
                }
                else
                {
                    changed = true;
                    Global.CurrentSettings.SeenMessages.Add(currentMessage.MessageID);
                }

                if (currentMessage.Action == string.Empty)
                {
                    ShowMessage(currentMessage.Text);
                }
                else
                {
                    Acr.UserDialogs.ConfirmConfig cc = new Acr.UserDialogs.ConfirmConfig();
                    cc.Message = currentMessage.Text;
                    cc.OkText = AppResources.minechat_general_yes;
                    cc.CancelText = AppResources.minechat_general_no;
                    cc.OnAction += delegate (bool result)
                    {
                        if (result == true)
                        {
                            try
                            {
                                Device.OpenUri(new Uri(currentMessage.Action));
                            }
                            catch
                            {
                                ShowMessage("Unable to open link");
                            }
                        }
                    };

                    Device.BeginInvokeOnMainThread(delegate
                    {
                        Acr.UserDialogs.UserDialogs.Instance.Confirm(cc);
                    });

                }
            }

            if (changed)
            {
                Global.SaveSettings();
            }

        }

        private void ShowBanner()
        {
            //appoDealView.ShowInterstatial();
        }


        private void HandleKeepalive()
        {
            Global.MineChatAPI.KeepAlive -= new MinecraftAPI.KeepAliveHandler(this.MineChatAPI_KeepAlive);

            if (Global.CurrentSettings.SpawnOnConnect && !this.sentSpawn)
            {
                Global.MineChatAPI.SendSpawn();
                this.sentSpawn = true;
            }

            if (!this.bannerSent)
            {
                this.bannerSent = true;
                if (Product.IsLite)
                {
                    if (this.lastLogin.ContainsKey(this.currentServer.ServerName) && DateTime.Now.Subtract(this.lastLogin[this.currentServer.ServerName]).TotalHours < 12.0)
                    {
                        return;
                    }

                    var device = DependencyService.Get<IDevice>();
                    Global.MineChatAPI.SendChat(AppResources.minechat_general_connectedwith);

                    if (this.lastLogin.ContainsKey(this.currentServer.ServerName))
                        this.lastLogin[this.currentServer.ServerName] = DateTime.Now;
                    else
                        this.lastLogin.Add(this.currentServer.ServerName, DateTime.Now);
                }
                else if (Global.CurrentSettings.LogonMessage != string.Empty)
                {
                    Global.MineChatAPI.SendChat(Global.CurrentSettings.LogonMessage);
                }
            }
        }

        private void EntryChat_OnHistoryForward(object sender, EventArgs e)
        {
            OnChatHistoryForward();
        }

        private void EntryChat_OnHistoryBackward(object sender, EventArgs e)
        {
            OnChatHistoryBackward();
        }

        private void OnChatHistoryForward()
        {
            if (chatHistoryIndex <= chatHistory.Count - 1)
            {
                chatHistoryIndex++;
                Debug.WriteLine("History:" + chatHistoryIndex);
                entryChat.Text = chatHistory.ElementAt(chatHistoryIndex - 1);
            }
        }

        private void OnChatHistoryBackward()
        {
            if (chatHistoryIndex - 1 >= 1)
            {
                chatHistoryIndex--;
                Debug.WriteLine("History:" + chatHistoryIndex);
                entryChat.Text = chatHistory.ElementAt(chatHistoryIndex - 1);
            }
            else
            {
                chatHistoryIndex = 0;
                entryChat.Text = string.Empty;
            }
        }

        private void EntryChat_OnTapped(object sender, EventArgs e)
        {
            //MainPage mp = (MainPage)App.Current.MainPage;
            //mp.ShowServers();
        }


    }
}
