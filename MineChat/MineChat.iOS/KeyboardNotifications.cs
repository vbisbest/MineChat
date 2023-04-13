using Foundation;
using UIKit;
using Xamarin.Forms;
using System;
using CoreGraphics;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.iOS.KeyboardNotifications))]
namespace MineChat.iOS
{
    public class KeyboardNotifications : IKeyboardNotifications
    {
        NSObject observerHideKeyboard;
        NSObject observerShowKeyboard;

        bool keyboardShown = false;
        nfloat offset = 0;
        nfloat keyboardHeight = 0;
        ContentPage topStack = null;

        StackLayout stackLayout = null;

        public event EventHandler OnKeyboardShown;
        public event EventHandler OnKeyboardHidden;

        public bool IsShown
        {
            get
            {
                return keyboardShown;
            }
        }

		public void HideKeyboard()
		{
			UIApplication.SharedApplication.KeyWindow.EndEditing(true);
		}

        public void HookKeyboard(StackLayout shiftableStack, ContentPage topStack)
        {
            this.topStack = topStack;

            stackLayout = shiftableStack;
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardWillHide);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyboardWillShow);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidHideNotification, KeyboardDidHide);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, KeyboardDidShow);
        }

        private void KeyboardDidShow(NSNotification notification)
        {
            Console.WriteLine("DidShow");
            keyboardShown = true;

            if (OnKeyboardShown != null)
            {
                OnKeyboardShown(this, null);
            }

        }

        private void KeyboardDidHide(NSNotification notification)
        {
            Console.WriteLine("DidHide");
            keyboardShown = false;

            if (OnKeyboardHidden != null)
            {
                OnKeyboardHidden(this, null);
            }
        }

        private void KeyboardWillShow(NSNotification notification)
        {
            if (keyboardShown)
            {
                return;
            }

            ShiftScreen(true, notification);
        }

        private void KeyboardWillHide(NSNotification notification)
        {
            if (!keyboardShown)
            {
                //return;
            }

            ShiftScreen(false, notification);
        }

        private void ShiftScreen(bool up, NSNotification notification)
        {
            CGRect keyboardFrame = UIKeyboard.BoundsFromNotification(notification);
            keyboardHeight = keyboardFrame.Height;

            NSNumber speedTemp = (NSNumber)notification.UserInfo.ObjectForKey(UIKeyboard.AnimationDurationUserInfoKey);
            System.nfloat speedConvert = (float)speedTemp;

            uint speed = System.Convert.ToUInt32(speedConvert * 1000);

            if (speed == 0)
            {
                return;
            }

            double newHeight = 0;

            if (up)
            {
                newHeight = topStack.Bounds.Height - (keyboardHeight + stackLayout.Bounds.Y);
                //newHeight = 10;
            }
            else
            {
                newHeight = topStack.Bounds.Height - stackLayout.Bounds.Y;
            }

            if (newHeight > UIScreen.MainScreen.Bounds.Height)
            {
                return;
            }

            Rectangle r = new Rectangle(stackLayout.Bounds.X, stackLayout.Bounds.Y, stackLayout.Bounds.Width, newHeight);
            Xamarin.Forms.ViewExtensions.LayoutTo(stackLayout, r, speed, null);
        }
    }
}
