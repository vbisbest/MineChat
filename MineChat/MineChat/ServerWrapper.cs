using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MineChatAPI;
using System.ComponentModel;
using Xamarin.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;

namespace MineChat
{
    [XmlInclude(typeof(Server))]
    public class ServerWrapper : Server, INotifyPropertyChanged
    {
        private ImageSource imageSource;
        private FormattedString formattedMOTD;
        private Server server = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public ServerWrapper(Server server)
        {
            this.server = server;
            InitInhertedProperties(server);
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitInhertedProperties(object baseClassInstance)
        {
            foreach (PropertyInfo propertyInfo in baseClassInstance.GetType().GetRuntimeProperties())
            {
                try
                {
                    object value = propertyInfo.GetValue(baseClassInstance, null);
                    if (propertyInfo.CanWrite)
                    {
                        if (null != value) propertyInfo.SetValue(this, value, null);
                    }
                }
                catch
                {
                    Debug.WriteLine("error");
                }
            }
        }

        public void Refresh()
        {
            
            //Debug.WriteLine("Refresh " + base.ServerName);
            CreateImageSource();
            CreateFormattedMOTD();

            this.OnPropertyChanged("MOTDColor");
            this.OnPropertyChanged("HasMOTD");
            this.OnPropertyChanged("MOTD");
            this.OnPropertyChanged("ServerName");
            this.OnPropertyChanged("ImageSource");       
            this.OnPropertyChanged("Address");
            this.OnPropertyChanged("Status");
            this.OnPropertyChanged("Info");
            this.OnPropertyChanged("FormattedMOTD");
        }
        
        public bool HasMOTD
        {
            get
            {
                bool r = (this.MOTD != null && this.MOTD != string.Empty);
                return r;
            }
        }

        public Color MOTDColor
        {
            get
            {
                if(base.Status == ServerStatus.Error)
                {
                    return Color.Red;
                }

                return Color.Yellow;
            }
        }

        public FormattedString FormattedMOTD
        {
            get
            {
                return formattedMOTD;
            }
        }

        public string FormattedAddress
        {
            get
            {
                if(this.ServerType == ServerTypeEnum.Realms)
                {
                    return "Minecraft Realm";
                }

                return this.FullAddress;
            }
        }

        private void CreateFormattedMOTD()
        {
            string temp = ChatProcessor.MinecraftStringToAttributedStringJson(base.MOTD);
            formattedMOTD = Global.MinecraftStringToFormattedString(temp, 12, string.Empty);
        }
          
        private void CreateImageSource()
        {
            try
            {
                if (base.FavIcon == null)
                {
                    this.imageSource = (ImageSource)null;
                }
                else
                {
                    var graphics = DependencyService.Get<IGraphics>();
                    byte[] scaled = graphics.ScaleImage(this.FavIcon, 60, 60);
                    if (scaled == null)
                    {
                        imageSource = (ImageSource)null;
                    }
                    else
                    {
                        imageSource = Global.BytesToImageSource(scaled);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error");
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
