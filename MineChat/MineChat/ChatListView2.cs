using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Linq;
using System.Diagnostics;

namespace MineChat
{
    public class ChatListView2 : ListView
    {
        public Action<int, int, bool> ScrollToRowDelegate { get; set; }

        public event EventHandler OnListViewScrolled;
        public event EventHandler<bool> ScrollToRequested;

        public ChatListView2 ()
        {            
        }

        public void ScrollToBottom(bool animated)
        {
            OnScrollToBottomRequested(animated);
        }

        void OnScrollToBottomRequested(bool animated)
        {
            var handler = ScrollToRequested;
            if (handler != null)
                handler(this, animated);
        }

        public void OnListViewScrolledEvent()
        {
            if (OnListViewScrolled != null)
            {
                OnListViewScrolled(this, null);
            }
        }

        public StackLayout Stack
        {
            get
            {
                return (StackLayout)this.Parent;
            }
        }


        public void ScrollToRow(int itemIndex, int sectionIndex = 0, bool animated = false)
        {
            if (ScrollToRowDelegate != null)
            {
                ScrollToRowDelegate(itemIndex, sectionIndex, animated);
            }
        }
    }
}