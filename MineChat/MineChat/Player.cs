using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Xamarin.Forms;
using MineChatAPI;
using System.Diagnostics;

namespace MineChat
{
    public class Player : IEquatable<Player>, INotifyPropertyChanged, IComparable<Player>
    {
        private string name;
        private ImageSource imageSource;
        private string cleanName;
        private bool triedToLoad;

        private FormattedString formattedName;
        //private byte[] skin;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(Player other)
        {
            if (other == null) return -1;
            return string.Compare(this.cleanName, other.cleanName, StringComparison.CurrentCultureIgnoreCase);
        }

        public string UUID { get; set; }
        public byte[] Skin { get; set; }

        public string CleanName
        {
            get
            {
                return this.cleanName;
            }
        }

        public bool HasSkin
        {
            get
            {
                return (imageSource != null);
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                this.cleanName = Global.MinecraftStringToPlain(this.name);
            }
        }

        public void Refresh()
        {
            CreateImageSource();
            CreateFormattedName();

            this.OnPropertyChanged("Name");
            this.OnPropertyChanged("ImageSource");
            this.OnPropertyChanged("FormattedMOTD");
            this.OnPropertyChanged("CleanName");
            
        }

        private void CreateFormattedName()
        {
            if (formattedName == null)
            {
                formattedName = Global.MinecraftStringToFormattedString(name, 12, string.Empty);
            }
        }


        private void CreateImageSource()
        {
            try
            {
                if (Skin == null)
                {
                    this.imageSource = (ImageSource)null;
                }
                else
                {
                    imageSource = Global.BytesToImageSource(Skin);
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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Player otherPlayer)
        {
            if (otherPlayer == null)
                return false;
            else
                return this.UUID.Equals(otherPlayer.UUID);
        }
    }
}
