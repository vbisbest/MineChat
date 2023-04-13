using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MineChat.Droid.KeyboardNotifications))]
namespace MineChat.Droid
{
    class KeyboardNotifications : IKeyboardNotifications
    {
        private bool keyboardShown;

        public bool IsShown
        {
            get
            {
                return keyboardShown;
            }
        }

        public event EventHandler OnKeyboardShown;
        public event EventHandler OnKeyboardHidden;

        public void HideKeyboard()
        {
            KeyboardHelper.HideKeyboard(GlobalDroid.mainActivity.FindViewById(Android.Resource.Id.Content));
        }

        public void HookKeyboard(StackLayout shiftableStack, ContentPage topStack)
        {
            
        }
    }
}