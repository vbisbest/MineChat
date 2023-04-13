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
    public partial class CommandsPage : ContentPage
    {
        ObservableCollection<CommandWrapper> commands = new ObservableCollection<CommandWrapper>();

        public CommandsPage()
        {
            InitializeComponent();
            this.Title = AppResources.minechat_general_commands;
            labelDescription.Text = AppResources.minechat_screens_commands;

            SetupPage();

        }

        private void SetupPage()
        {
            try
            {
                listView.HasUnevenRows = true;
                listView.IsGroupingEnabled = true;
                //listView.GroupDisplayBinding = new Binding("Key");

                ToolbarItem addCommand = new ToolbarItem();
                addCommand.Clicked += AddCommand_Clicked;
                addCommand.Icon = "add.png";
                addCommand.Text = AppResources.minechat_settings_add;

                // Set the list view source
                SetSource();

                this.ToolbarItems.Add(addCommand);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // An account was selected, show the actions
            //if (e.SelectedItem == null)
            //{
            //    return;
            //}

            //CommandWrapper selectedCommand = (CommandWrapper)e.SelectedItem;
            //DisplayActions(selectedCommand);
        }

        private string GetTypeString(MineChatAPI.CommandType type)
        {
            if(type == CommandType.Player)
            {
                return AppResources.minechat_general_player;
            }

            return AppResources.minechat_general_server;

        }

        private void SetSource()
        {
            listView.ItemsSource = null;

            commands.Clear();

            foreach (MineChatAPI.Command currentCommand in Global.CurrentSettings.Commands)
            {
                CommandWrapper cw = new CommandWrapper();
                cw.Command = currentCommand;
                commands.Add(cw);
            }

            
            List<KeyedList<string, MineChatAPI.Command>> l = new List<KeyedList<string, MineChatAPI.Command>>(Enumerable.Select<IGrouping<string, MineChatAPI.Command>, KeyedList<string, MineChatAPI.Command>>(Enumerable.GroupBy<MineChatAPI.Command, string>((IEnumerable<MineChatAPI.Command>)this.commands, (Func<MineChatAPI.Command, string>)(list => GetTypeString(list.Type))), (Func<IGrouping<string, MineChatAPI.Command>, KeyedList<string, MineChatAPI.Command>>)(listByGroup => new KeyedList<string, MineChatAPI.Command>(listByGroup))));
            IEnumerable sorted = l.OrderByDescending(s => s[0].Type);
            listView.ItemsSource = sorted;
        }

        private void AddCommand_Clicked(object sender, EventArgs e)
        {
            AddCommand();
        }

        private void SaveCommand(CommandWrapper command)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!commands.Contains(command))
                {
                    // Add account
                    commands.Add(command);
                }

                SaveCommands();
            });
        }

        private async void ShowEditCommand(MineChatAPI.Command command, bool adding)
        {
            CommandAddPage commandAddPage = new CommandAddPage();
            commandAddPage.BindingContext = command;

            commandAddPage.Disappearing += delegate
            {
                // Came back from editing now save
                if (commandAddPage.Save)
                {
                    
                    if (adding)
                    {
                        Global.CurrentSettings.Commands.Add(command);
                    }

                    Global.SaveSettings();
                    SetSource();
                }
            };

            // Push the dialog
            await Navigation.PushAsync(commandAddPage);
        }

        private void AddCommand()
        {
            if (Product.IsLite)
            {
                if (Global.CurrentSettings.Commands.Count >= 4)
                {
                    ShowMessage(AppResources.minechat_settings_commandslite);
                    return;
                }
            }


            MineChatAPI.Command account = new MineChatAPI.Command();
            ShowEditCommand(account, true);
        }

        public void OnEdit(object sender, EventArgs e)
        {
            CommandWrapper sw = (sender as Xamarin.Forms.MenuItem).BindingContext as CommandWrapper;
            MineChatAPI.Command editCommand = Global.CurrentSettings.Commands.Find(c => c.CommandText == sw.CommandText);
            ShowEditCommand(editCommand, false);
        }

        public void OnDelete(object sender, EventArgs e)
        {
            try
            {
                CommandWrapper command = (sender as Xamarin.Forms.MenuItem).BindingContext as CommandWrapper;
                Global.CurrentSettings.Commands.RemoveAll(s => s.CommandText == command.CommandText);
                Global.SaveSettings();
                SetSource();
            }
            catch
            {
                Debug.WriteLine("Remove failed");
            }
        }

        private void SaveCommands()
        {
            Global.CurrentSettings.Commands.Clear();

            // Push the accounts to the real settings
            foreach (CommandWrapper currentCommand in commands)
            {
                Global.CurrentSettings.Commands.Add(currentCommand.Command);
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
