using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.ComponentModel;
using System.Diagnostics;

namespace MineChat
{
    public class ActionBarMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string actionBarText = string.Empty;
        private FormattedString formattedMessage = null;
        private bool isVisible = false;
        private Stopwatch stopWatch = new Stopwatch();
        private bool reset = false;
        private bool timerStarted = false;

        public ActionBarMessage()
        {
            
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsVisible
        {
            get
            {
                if(formattedMessage==null)
                {
                    return false;
                }

                if(formattedMessage.Spans.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        
        public string ActionBarText
        {
            set
            {
                actionBarText = value;
                CreateFormatteMessage();
            }
        }
        

        public FormattedString FormattedActionBarText
        {
            get
            {
                if(formattedMessage==null)
                {
                    formattedMessage = new FormattedString();
                }

                return formattedMessage;
            }
            set
            {
                try
                {
                    formattedMessage = value;
                    reset = true;

                    if (!timerStarted)
                    {
                        timerStarted = true;
                        Device.StartTimer(System.TimeSpan.FromSeconds(5), () =>
                        {
                            if (reset == false)
                            {
                                formattedMessage = null;
                                OnPropertyChanged("IsVisible");
                                OnPropertyChanged("FormattedActionBarText");

                                timerStarted = false;

                                return false;
                            }
                            else
                            {
                                reset = false;
                                return true;
                            }
                        });                       
                    }

                    OnPropertyChanged("ActionBarText");
                    OnPropertyChanged("FormattedActionBarText");
                    OnPropertyChanged("IsVisible");

                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void CreateFormatteMessage()
        {
            if (formattedMessage == null)
            {
                if (formattedMessage == null)
                {
                    string temp = ChatProcessor.MinecraftStringToAttributedStringJson(actionBarText);
                    formattedMessage = Global.MinecraftStringToFormattedString(temp, Global.FontSizes[Global.CurrentSettings.FontSize], Global.Fonts[Global.CurrentSettings.Font]);
                }
                else
                {
                    formattedMessage = null;
                }

                OnPropertyChanged("ActionBarText");
                OnPropertyChanged("FormattedActionBarText");
                OnPropertyChanged("IsVisible");
            }
            else
            {
                formattedMessage = null;
            }
        }

    }
}
