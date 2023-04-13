using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MineChat
{
    public class TestListView : ListView 
    {
        public Action<bool> ScrollToBottomDelegate { get; set; }
        public Action<ChatMessage> AddMessageDelegate { get; set; }
        public Action ClearMessagesDelegate { get; set; }

        private ObservableCollection<ChatMessage> chatMessages;

        public static readonly BindableProperty ItemsProperty = BindableProperty.Create ("Items", typeof (IEnumerable<ChatMessage>), typeof (TestListView), new List<ChatMessage> (), BindingMode.TwoWay );

        public IEnumerable<ChatMessage> Items 
        {
            get 
            { 
                return (IEnumerable<ChatMessage>)GetValue (ItemsProperty); 
            }
            set 
            { 
                SetValue (ItemsProperty, value); 
            }
        }

        public TestListView (ListViewCachingStrategy strategy) : base (strategy)
        {
            
        }

        public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

        public void Init()
        {

            chatMessages = new ObservableCollection<ChatMessage>();
            this.ItemsSource = chatMessages;
            ClearMessages();

        }

        public void NotifyItemSelected (object item)
        {

            if (ItemSelected != null)
                ItemSelected (this, new SelectedItemChangedEventArgs (item));
        }

        public void ScrollToBottom(bool force)
		{
            if (ScrollToBottomDelegate != null)
            {
                ScrollToBottomDelegate(force);
            }
        }

        public void ScrollToBottom()
        {
           ScrollToBottom(false);
        }

        public void AddMessage (ChatMessage message)
        {
            if (Device.OS != TargetPlatform.Android)
            {
                this.BeginRefresh();
            }

            chatMessages.Add(message);

            if (AddMessageDelegate != null)
            {
                AddMessageDelegate(message);
            }

            if (Device.OS != TargetPlatform.Android)
            {
                this.EndRefresh();
            }

            ScrollToBottom(false);
        }

        public void ClearMessages()
        {
            chatMessages.Clear();
            if (ClearMessagesDelegate != null)
            {
                ClearMessagesDelegate();
            }

        }
    }
}
