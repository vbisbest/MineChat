using Xamarin.Forms.Platform.iOS;
using MineChat;
using Xamarin.Forms;
using MineChat.iOS;

[assembly: ExportRenderer(typeof(ChatPage), typeof(ChatPageRenderer))]
namespace MineChat.iOS
{
    public class ChatPageRenderer : PageRenderer
    {
        ChatPage chatPage = null;

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {

            
            

            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                // Unsubscribe
            }

            if (e.NewElement != null)
            {
                chatPage = (ChatPage)e.NewElement;
                GlobaliOS.contentPage = chatPage;

                e.NewElement.BackgroundColor = Color.Black;
            }
        }
    }
}
