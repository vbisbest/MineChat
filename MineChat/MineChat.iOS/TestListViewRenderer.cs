using System.Collections.Generic;
using UIKit;
using MineChat.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using MineChat;
using System.Linq;
using Foundation;
using System.Diagnostics;
using System;
using CoreGraphics;
using AVFoundation;


[assembly: ExportRenderer (typeof (TestListView), typeof (TestListViewRenderer))]
namespace MineChat.iOS
{
    public class TestListViewRenderer : ViewRenderer<TestListView, UITableView>
    {
        void ListView_ClearMessagesDelegate()
        {
			source.ClearMessages();
			Control.ReloadData();
        }

        void ListView_AddMessageDelegate(ChatMessage message)
        {
			try
			{
				if (source != null)
				{
					source.AddMessage(message);
					Control.ReloadData();
				}
			}
			catch
			{
				//ignore
			}
        }

        void ListView_ScrollToBottomDelegate(bool force)
        {
            ScrollToBottom(force);
        }

        TestListViewSource source = null;
        TestListView listView = null;

        public TestListViewRenderer()
        {
        }

        protected override void OnElementChanged (ElementChangedEventArgs<TestListView> e)
        {
           
            base.OnElementChanged (e);

            if (Control == null) 
            {
                SetNativeControl (new UITableView ());
            }

			if (e.OldElement != null)
			{
				// Cleanup resources and remove event handlers for this element.
				listView.ScrollToBottomDelegate -= ListView_ScrollToBottomDelegate;
				listView.AddMessageDelegate -= ListView_AddMessageDelegate;
			}

			if (e.NewElement != null) 
            {
                listView = e.NewElement as TestListView;
                listView.ScrollToBottomDelegate += ListView_ScrollToBottomDelegate;
                listView.AddMessageDelegate += ListView_AddMessageDelegate;  

                e.NewElement.ClearMessagesDelegate = () =>
                {
                    try
                    {
                        if (source != null)
                        {
                            source.ClearMessages();
                            Control.ReloadData();
                        }
                    }
                    catch
                    {
                        //ignore
                    }


                };

                Control.SeparatorStyle = UITableViewCellSeparatorStyle.None;
                Control.BackgroundColor = UIColor.Black;

                source = new TestListViewSource(e.NewElement, Control);
                source.Items = e.NewElement.Items;
                Control.Source = source;
            }

           
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            //Debug.WriteLine(e.PropertyName);

            if (e.PropertyName == TestListView.ItemsProperty.PropertyName)
            {
                Control.ReloadData();
            }
        }

        private void ScrollToBottom(bool force)
        {
            
            if (Control != null)
            {
                //Control.ReloadData();
                if (source.Items.Count() == 0)
                {
                    return;
                }

                try
                {
                    NSIndexPath np = NSIndexPath.FromRowSection(source.Items.Count() - 1, 0);
                    Control.ScrollToRow(np, UITableViewScrollPosition.Bottom, true);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

  
    public class TestListViewSource : UITableViewSource
    {
        // declare vars
        IList<ChatMessage> tableItems;
        string cellIdentifier = "TableCell";
        TestListView listView;
        UITableView tableView;
        public bool PinToBottom = true;

        public IEnumerable<ChatMessage> Items 
        {
            set 
            {
                tableItems = (List<ChatMessage>)value;
            }
            get 
            {
                return tableItems;
            }
        }

        public void AddMessage (ChatMessage message)
        {
            tableItems.Add(message);
        }

        public void ClearMessages()
        {
            tableItems.Clear();
        }

        public override void ScrolledToTop(UIScrollView scrollView)
        {
            PinToBottom = false;
        }

        public TestListViewSource (TestListView view, UITableView tbv)
        {
            tableItems = view.Items.ToList ();
            listView = view;
            tableView = tbv;
        }

        /// <summary>
        /// Called by the TableView to determine how many cells to create for that particular section.
        /// </summary>
        public override nint RowsInSection (UITableView tableview, nint section)
        {
            return tableItems.Count;
        }

        #region user interaction methods

        public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
        {
            try
            {
                listView.NotifyItemSelected(tableItems[indexPath.Row]);
                tableView.DeselectRow(indexPath, true);
            }
            catch
            {
                // ignore
            }
        }

        public override void RowDeselected (UITableView tableView, NSIndexPath indexPath)
        {
        }

        #endregion

        /// <summary>
        /// Called by the TableView to get the actual UITableViewCell to render for the particular section and row
        /// </summary>
        public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
        {
            // declare vars
            UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);

            ChatMessage chatMessage = null;
            try
            {
                chatMessage = (ChatMessage)tableItems[indexPath.Row];
            }
            catch
            {
                return null;
            }

            // if there are no cells to reuse, create a new one
            if (cell == null) 
            {
                UILabel label = new UILabel();
                label.LineBreakMode = UILineBreakMode.WordWrap;
                label.Lines = 0;
                cell = new MyCustomCell(label);
            }

            MyCustomCell cc = (MyCustomCell)cell;

            cc.Label.AttributedText = chatMessage.FormattedMessage.ToAttributed(Font.Default, Color.White);

            // set the item text
            cc.Label.SizeToFit();
            cell.SizeToFit();

            nfloat width = this.tableView.Bounds.Width - Global.WidthOffset;
            cc.Label.Frame = new CGRect(Global.XOffset, 0, width, 0);

            cc.Label.SizeToFit();
            cc.Label.Frame = new CGRect(Global.XOffset, 0, width, cc.Label.Frame.Height);


            if (chatMessage.IsAlert)
            {
                cell.BackgroundColor = UIColor.FromRGB(112, 0, 0);
            }
            else
            {
                cell.BackgroundColor = UIColor.Clear;
            }

            return cell;
        }

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {

            try
            {
                nfloat bottomEdge = tableView.ContentOffset.Y + tableView.Frame.Size.Height;
                if (bottomEdge >= tableView.ContentSize.Height)
                {
                    PinToBottom = true;
                }
                else
                {
                    PinToBottom = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not pin to bottom: " + ex.Message);
            }
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            ChatMessage message = (ChatMessage)tableItems[indexPath.Row];
            //Console.WriteLine("GetEstimatedHeightForRowStored: " + message.RowHeight.ToString());

            if (message.RowHeight == 0)
            {
                GetRowHeight(message);
            }

            return message.RowHeight;

        }

        public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
        {
            ChatMessage message = (ChatMessage)tableItems[indexPath.Row];
            GetRowHeight(message);
            //Console.WriteLine("GetHeightForRow: " + message.RowHeight.ToString());
            return message.RowHeight;
        }

        private void GetRowHeight(ChatMessage message)
        {
            UILabel label = new UILabel();

            label.Lines = 0;
            label.LineBreakMode = UILineBreakMode.WordWrap;

            label.AttributedText = message.FormattedMessage.ToAttributed(Font.Default, Color.Black);

            if (label.Frame.Width != tableView.Bounds.Width - Global.WidthOffset)
            {
                label.Frame = new CGRect(Global.XOffset, 0, tableView.Bounds.Width - Global.WidthOffset, label.Frame.Height);
            }

            label.SizeToFit();
            message.RowHeight = Convert.ToInt32(label.Frame.Height + 14);           
        }

        public class MyCustomCell : UITableViewCell
        {
            
            private UILabel currentLabel = null;

            public UILabel Label 
            { 
                get
                {
                    return currentLabel;
                }
            }

            public MyCustomCell(UILabel label)
            {
                currentLabel = label;
                this.ContentView.AddSubview(label);
            }
        }
    }
}