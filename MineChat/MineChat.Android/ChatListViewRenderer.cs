using System;
using Xamarin.Forms;
using MineChat;
using Android.App;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using MineChat.Droid;
using Android.Content;

[assembly: ExportRenderer(typeof(TestListView), typeof(ChatListViewRenderer))]
namespace MineChat.Droid
{
    public class ChatListViewRenderer : ListViewRenderer
    {
        bool pinToBottom = true;
        bool touchScroll = false;
        TestListView listView;
        Context localContext;

        public ChatListViewRenderer(Context context)
            : base(context) {
            localContext = context;
        }


        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // Unsubscribe
                listView.ScrollToBottomDelegate -= ListView_ScrollToBottomDelegate;
            }

            if (e.NewElement != null)
            {
                Control.ScrollStateChanged += Control_ScrollStateChanged;
                Control.Scroll += Control_Scroll;


                var window = ((Activity)localContext).Window;
                window.SetSoftInputMode(SoftInput.AdjustResize);

                listView = e.NewElement as TestListView;
                listView.BackgroundColor = Color.Black;
                listView.HasUnevenRows = true;
                listView.ScrollToBottomDelegate += ListView_ScrollToBottomDelegate;
            }
        }

        void ListView_ScrollToBottomDelegate(bool force)
        {
            ScrollToBottom(force);
        }

        private void Control_Scroll(object sender, AbsListView.ScrollEventArgs e)
        {
            if (Global.IsConnected && touchScroll)
            {
                touchScroll = false;
                if (Control.LastVisiblePosition == Control.Adapter.Count - 1 && Control.GetChildAt(Control.ChildCount - 1).Bottom <= Control.Height)
                {
                    //scroll end reached
                    pinToBottom = true;
                    //Console.WriteLine("Pin to Bottom");
                }
                else
                {
                    pinToBottom = false;
                    //Console.WriteLine("Dont Pin to Bottom");
                }
            }
        }

        private void Control_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            touchScroll = false;

            if(Control==null || Control.Adapter == null)
            {
                return;
            }

            if (e.ScrollState == ScrollState.TouchScroll)
            {
                //System.Diagnostics.Debug.WriteLine("touch scroll");
                touchScroll = true;
                //listView.OnListViewScrolledEvent();
                return;
            }

            if (Control.LastVisiblePosition == Control.Adapter.Count - 1 && Control.GetChildAt(Control.ChildCount - 1).Bottom <= Control.Height)
            {
                //scroll end reached
                pinToBottom = true;
                //Console.WriteLine("Pin to Bottom");
            }
        }

        private void ScrollToRow(int itemIndex, int sectionIndex, bool animated)
        {
            if (itemIndex == -1 || Control == null)
            {
                return;
            }

            
            if (pinToBottom)
            {
                //Console.WriteLine("ScrollToRow");
                Control.SmoothScrollToPositionFromTop(Control.Adapter.Count - 1, 0, 1);
            }
        }


        private void ScrollToBottom(bool force)
        {
            if (Control == null)
            {
                return;
            }


            if (pinToBottom)
            {
                //Console.WriteLine("ScrollToRow");
                Control.SmoothScrollToPositionFromTop(Control.Adapter.Count - 1, 0, 1);
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }
    }
}