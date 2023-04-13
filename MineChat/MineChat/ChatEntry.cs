using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Diagnostics;
using MineChat;

namespace MineChat
{
    public class ChatEntry : Entry
    {
        public Action HideKeyboardDelegate { get; set; }
        public Action ConfigureEntryDelegate { get; set; }
        public Action DetachEntryDelegate { get; set; }

        public event EventHandler OnSendChat;
        public event EventHandler OnHistoryForward;
        public event EventHandler OnHistoryBackward;
        public event EventHandler OnTapped;

        public ChatEntry()
        {
        }

        public string HintConnectToServer
        {
            get
            {
                return MineChat.Languages.AppResources.minechat_servers_connect;
            }
        }

        public void SendChat()
        {
            if (OnSendChat != null)
            {
                OnSendChat(this, null);
            }
        }        

        public void Detach()
        {
            if (DetachEntryDelegate != null)
            {
                DetachEntryDelegate();
            }
        }     

        public void HistoryForward()
        {
            if (OnHistoryForward != null)
            {
                OnHistoryForward(this, null);
            }
        }

        public void HistoryBackward()
        {
            if (OnHistoryBackward != null)
            {
                OnHistoryBackward(this, null);
            }
        }

        public void HideKeyboard()
        {
            if (HideKeyboardDelegate != null)
            {
                HideKeyboardDelegate();
            }
        }

        public void ConfigureEntry()
        {
            if (ConfigureEntryDelegate != null)
            {
                ConfigureEntryDelegate();
            }
        }

        public void Tapped()
        {
            if (OnTapped != null)
            {
                OnTapped(this, null);
            }
        }
    }
}
