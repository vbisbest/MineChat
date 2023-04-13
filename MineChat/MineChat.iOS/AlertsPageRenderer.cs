﻿using MineChat;
using Xamarin.Forms;
using MineChat.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AlertsPage), typeof(AlertsPageRenderer))]
namespace MineChat.iOS
{
    public class AlertsPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                //e.NewElement.BackgroundColor = Color.Black;
            }
        }
    }
}
