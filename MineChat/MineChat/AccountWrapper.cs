using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineChatAPI;
using System.ComponentModel;
using Xamarin.Forms;

namespace MineChat
{
    public class AccountWrapper : Account, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ImageSource imageSource;

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        public Account Account
        {
            get
            {
                Account account = new Account();
                account.UserName = base.UserName;
                account.Password = base.Password;
                account.Selected = base.Selected;
                account.Skin = base.Skin;
                account.PlayerName = base.PlayerName;
                account.AccessToken = base.AccessToken;
                account.ClientToken = base.ClientToken;
                account.ProfileID = base.ProfileID;
                account.IsOffline = base.IsOffline;
                return account;
            }
            set
            {
                base.UserName = value.UserName;
                base.Password = value.Password;
                base.Selected = value.Selected;
                base.Skin = value.Skin;
                base.PlayerName = value.PlayerName;
                base.AccessToken = value.AccessToken;
                base.ClientToken = value.ClientToken;
                base.ProfileID = value.ProfileID;
                base.IsOffline = value.IsOffline;

                CreateImageSource();
            }
        }

        public void Refresh()
        {
            CreateImageSource();
            this.OnPropertyChanged("PlayerName");
            this.OnPropertyChanged("UserName");
            this.OnPropertyChanged("Password");
            this.OnPropertyChanged("Selected");
            this.OnPropertyChanged("ImageSource");
        }

        private void CreateImageSource()
        {
            try
            {
                if (base.Skin == null)
                {
                    this.imageSource = (ImageSource)null;
                }
                else
                {
                    byte[] bytes = System.Convert.FromBase64String(base.Skin);
                    imageSource = Global.BytesToImageSource(bytes);
                }
            }
            catch
            {
            }

        }

        public ImageSource ImageSource
        {
            get
            {
                return imageSource;
            }
        }
    }
}
