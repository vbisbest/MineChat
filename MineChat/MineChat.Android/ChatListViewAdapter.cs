using System;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;
using System.Collections;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using MineChat.Droid;

namespace MineChat.Droid
{
    public class ChatListViewAdapter : BaseAdapter<string>
    {

        readonly Activity context;
        IList<string> tableItems = new List<string>();

        public IEnumerable<string> Items
        {
            set
            {
                tableItems = value.ToList();
            }
        }

        public ChatListViewAdapter(Activity context, string[] items) : base() {
            this.context = context;
            this.tableItems = items.ToList();
        }

        public override string this[int position]
        {
            get
            {
                return tableItems[position];
            }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return tableItems.Count; }
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
        {
            // Get our object for this position
            var item = this.tableItems[position];

            var view = convertView;
            if (view == null)
            { // no view to re-use, create new
                view = context.LayoutInflater.Inflate(global::Android.Resource.Layout.SimpleListItem1, null);
            }

            view.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = item;

            return view;
        }        
    }
}