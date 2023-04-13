using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MineChat.Languages;
using Xamarin.Forms;
using MineChatAPI;
using Acr.UserDialogs;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MineChat
{
    public partial class AccountAddPage : ContentPage
    {
        private bool save = false;

        public AccountAddPage()
        {
            InitializeComponent();

            this.Title = AppResources.minechat_general_accounts;
            
            //labelUsername. = AppResources.minechat_settings_username;
            labelPassword.Text = AppResources.minechat_settings_password;
            labelOffline.Text = AppResources.minechat_general_offline;

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
                ToolbarItem saveToolbar = new ToolbarItem();
                saveToolbar.Clicked += Save_Clicked;
                saveToolbar.Text = AppResources.minechat_general_save;
                saveToolbar.Icon = "save.png";
                
                this.ToolbarItems.Add(saveToolbar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        private void ValidateAccount(AccountWrapper account)
        {
            UserDialogs.Instance.ShowLoading("Validating");

            Task.Run(() =>
            {
                try
                {
                    MinecraftAPI.AuthenticateAccount(account, true);
                    GetPlayerInfo(account);
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var analytics = DependencyService.Get<IAnalytics>();
                        if (analytics != null)
                        {
                            analytics.FailedLogin(ex.GetBaseException().Message);
                        }


                        UserDialogs.Instance.HideLoading();
                        ShowMessage(AppResources.minechat_general_errorvalidate + ": " + ex.GetBaseException().Message);
                    });
                }
            });
        }

        
        private void GetPlayerInfo(AccountWrapper account)
        {

            MojangAPI api = new MojangAPI();

            api.PlayerInfoCompleted += delegate (PlayerInfo playerInfo)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (playerInfo != null && playerInfo.textureObject.Textures.Skin.Bytes != null)
                    { 
                        // New account
                        var graphics = DependencyService.Get<IGraphics>();
                        try
                        {
                            byte[] cropped = graphics.CropImage(Convert.FromBase64String(playerInfo.textureObject.Textures.Skin.Bytes), 8, 8, 8, 8);
                            byte[] resized = graphics.ScaleImage(cropped, 60, 60);
                            
                            account.Skin = System.Convert.ToBase64String(resized);
                        }
                        catch
                        {
                            account.Skin = null;
                        }
                    }                
                    else
                    {
                        account.Skin = null;
                    }

                    save = true;
                    Close();
                    
                });

            };

            api.GetPlayerInfo(account.ProfileID, string.Empty);
        }
        

        private void Save_Clicked(object sender, EventArgs e)
        {
            var account = (AccountWrapper)this.BindingContext;

            if (!account.IsOffline)
            {
                if (!account.UserName.Contains("@"))
                {
                    ShowMessage("Username must be an email address. Make sure you use the same email address that you use in Minecraft");
                    return;
                }
                else if(account.Password == string.Empty)
                {
                    ShowMessage("Password cannot be blank");
                    return;
                }

                ValidateAccount(account);
            }
            else
            {
                save = true;
                Close();
            }
        }

        private void Close()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.HideLoading();

                Debug.WriteLine(Navigation.NavigationStack.Count);

                if (Navigation.NavigationStack.Count > 1)
                {
                    RealClose();
                }
                else
                {
                    var analytics = DependencyService.Get<IAnalytics>();
                    if (analytics != null)
                    {
                        analytics.LogError("Could not add account, stack = 0");
                    }
                    //ShowMessage("Error adding account.  Please email support support@minechatapp.com");
                }
            });
        }

        private async void RealClose()
        {
            await Navigation.PopAsync(true);
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
