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
    public partial class MasterPage : ContentPage
    {
        public ListView ListView { get { return listView; } }
        public List<MasterPageItem> masterPageItems = new List<MasterPageItem>();


        public MasterPage()
        {
            InitializeComponent();
            this.BackgroundColor = Global.MainColor;

            masterPageItems.Add(new MasterPageItem
            {
                Title = AppResources.minechat_general_chat,
                IconSource = "chat.png",
                TargetType = typeof(ChatPage)
            });


            masterPageItems.Add(new MasterPageItem
            {
                Title = AppResources.minechat_general_servers,
                IconSource = "servers.png",
                TargetType = typeof(ServersPage),
            });

            masterPageItems.Add(new MasterPageItem
            {
                Title = AppResources.minechat_general_commands,
                IconSource = "command.png",
                TargetType = typeof(CommandsPage)
            });

            masterPageItems.Add(new MasterPageItem
            {
                Title = AppResources.minechat_general_alerts,
                IconSource = "alert.png",
                TargetType = typeof(AlertsPage)
            });

            masterPageItems.Add(new MasterPageItem
            {
                Title = AppResources.minechat_general_accounts,
                IconSource = "account.png",
                TargetType = typeof(AccountsPage)
            });

           masterPageItems.Add(new MasterPageItem
           {
               Title = AppResources.minechat_general_settings,
               IconSource = "settings.png",
               TargetType = typeof(SettingsPage)
           });

           masterPageItems.Add(new MasterPageItem
           {
               Title = AppResources.minechat_settings_about,
               IconSource = "about.png",                
               TargetType = typeof(AboutPage)
           });
           

            listView = new ListView
            {
                ItemsSource = masterPageItems,
                ItemTemplate = new DataTemplate(() => {
                    var imageCell = new ImageCell();
                    imageCell.SetBinding(TextCell.TextProperty, "Title");
                    imageCell.SetBinding(ImageCell.ImageSourceProperty, "IconSource");
                    imageCell.TextColor = Color.White;
                    return imageCell;

                }),
                BackgroundColor = Global.MainColor,
                VerticalOptions = LayoutOptions.FillAndExpand,
                SeparatorVisibility = SeparatorVisibility.None
            };

            Padding = new Thickness(0, 40, 0, 0);
            Icon = "menu.png";
            Title = Product.ProductName;
            listView.HasUnevenRows = false;

            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    listView
                }
            };
            
        }
    }
}
