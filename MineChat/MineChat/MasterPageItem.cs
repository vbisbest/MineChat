using System;
using Xamarin.Forms;

namespace MineChat
{
	public class MasterPageItem
	{
		public string Title { get; set; }
		public string IconSource { get; set; }
		public Type TargetType { get; set; }
        public Page CurrentInstance { get; set; }
	}
}
