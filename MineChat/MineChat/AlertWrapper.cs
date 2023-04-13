using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineChatAPI;
using System.ComponentModel;
using Xamarin.Forms;

namespace MineChat
{
    public class AlertWrapper : MineChatAPI.AlertWord, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        public MineChatAPI.AlertWord AlertWord
        {
            get
            {
                MineChatAPI.AlertWord alertWord = new MineChatAPI.AlertWord();
                alertWord.Word = base.Word;
                alertWord.Vibrate = base.Vibrate;
                alertWord.Sound = base.Sound;
                return alertWord;
            }
            set
            {
                base.Word = value.Word;
                base.Vibrate = value.Vibrate;
                base.Sound = value.Sound;
            }
        }

        public void Refresh()
        {
            this.OnPropertyChanged("Word");
            this.OnPropertyChanged("Vibrate");
            this.OnPropertyChanged("Sound");
        }
    }
}