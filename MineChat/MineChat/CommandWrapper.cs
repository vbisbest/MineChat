using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineChatAPI;
using System.ComponentModel;
using Xamarin.Forms;

namespace MineChat
{
    public class CommandWrapper : MineChatAPI.Command, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        public MineChatAPI.Command Command
        {
            get
            {
                MineChatAPI.Command command = new MineChatAPI.Command();
                command.CommandText = base.CommandText;
                command.Type = base.Type;
                return command;
            }
            set
            {
                base.CommandText = value.CommandText;
                base.Type = value.Type;
            }
        }

        public string TypeString 
        {
            get
            {
                return base.TypeString;
            }
        }

        public void Refresh()
        {
            this.OnPropertyChanged("Type");
            this.OnPropertyChanged("CommandText");
        }
    }
}
