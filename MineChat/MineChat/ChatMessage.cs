using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MineChat
{
    public class ChatMessage : INotifyPropertyChanged
    {
        private string message;
        private bool isAlert;
        private string color;

        private FormattedString formattedMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        public int RowHeight { get; set; }
        public MineChatAPI.ChatType ChatType { get; set; }

        public string GetLink
        {
            get
            {
                string link = string.Empty;

                if(this.Message == null)
                {
                    return string.Empty;
                }

                try
                {
                    JContainer x = (JContainer)JsonConvert.DeserializeObject(this.Message);
                    JObject t = (JObject)x.SelectTokens("..clickEvent").Where(s => (string)s["action"] == "open_url").FirstOrDefault();
                    if (t != null)
                    {
                        link = t.GetValue("value").ToString();
                        return link;
                    }
                }
                catch
                {
                    // ignore
                }

                string[] words = this.CleanMessageKeepCase.Split(" ".ToCharArray());

                foreach (string currentWord in words)
                {
                    if (currentWord.ToLower().Contains(".ly") || currentWord.ToLower().Contains(".co") || currentWord.ToLower().Contains(".org") || currentWord.ToLower().Contains(".net") || currentWord.ToLower().Contains(".edu") || currentWord.ToLower().Contains(".town") || currentWord.ToLower().Contains(".rocks"))
                    {
                        if (!currentWord.ToLower().Contains("http"))
                        {
                            link = "http://" + currentWord;
                        }
                        else
                        {
                            link = currentWord;
                        }
                    }
                    else if (currentWord.ToLower().StartsWith("http"))
                    {
                        link = currentWord;
                    }
                }

                return link;
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = value;
                CreateFormatteMessage();
            }
        }

        public bool IsAlert
        {
            get
            {
                return this.isAlert;
            }
            set
            {
                if (value == true)
                {
                    color = "#990000";
                }
                else
                {
                    color = "Transparent";
                }

                this.isAlert = value;
            }
        }

        public FormattedString FormattedMessage
        {
            get
            {
                return formattedMessage;
            }
        }

        public double FontSizeValue
        {
            get
            {
                return Convert.ToDouble(Global.FontSizes[Global.CurrentSettings.FontSize]);
            }
        }

        public string FontValue
        {
            get
            {
                return Global.Fonts[Global.CurrentSettings.Font];
            }
        }

        public string CleanMessage
        {
            get
            {
                string temp = ChatProcessor.MinecraftStringToAttributedStringJson(message);
                return Global.MinecraftStringToPlain(temp.ToLower());
            }
        }

        public string CleanMessageKeepCase
        {
            get
            {
                string temp = ChatProcessor.MinecraftStringToAttributedStringJson(message);
                return Global.MinecraftStringToPlain(temp);
            }
        }

        private void CreateFormatteMessage()
        {
           if (formattedMessage == null)
           {
                string temp = ChatProcessor.MinecraftStringToAttributedStringJson(message);
                formattedMessage = Global.MinecraftStringToFormattedString(temp, Global.FontSizes[Global.CurrentSettings.FontSize], Global.Fonts[Global.CurrentSettings.Font]);
            }
           else
            {
                formattedMessage = null;
            }
        }

        public string Color
        {
            get
            {
                return this.color;
            }
        }
    }
}


