using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using MineChat.iOS;
using UIKit;
using MineChat;


[assembly: ExportRenderer(typeof(TransparentViewCell), typeof(TransparentViewCellRenderer))]

namespace MineChat.iOS
{
    public class TransparentViewCellRenderer : ViewCellRenderer
    {
        public TransparentViewCellRenderer()
        {
        }

        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);
            if (cell != null) cell.BackgroundColor = UIColor.Clear;
            return cell;
        }
    }
}