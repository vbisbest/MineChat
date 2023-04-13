using System;
using System.Reflection;
using Android.Text;
using Xamarin.Forms;
using Android.Views.InputMethods;
using Android.Content;
using System.Collections.Generic;

using View = Android.Views.View;

namespace MineChat.Droid
{
    public static class KeyboardHelper
    {
        private const string KeyboardExtensionsType = "Xamarin.Forms.Platform.Android.KeyboardExtensions, Xamarin.Forms.Platform.Android";
        private const string KeyboardExtensionsToInputType = "ToInputType";
        private const string KeyboardManagerType = "Xamarin.Forms.Platform.Android.KeyboardManager, Xamarin.Forms.Platform.Android";
        private const string KeyboardManagerHideKeyboard = "HideKeyboard";
        private const string KeyboardManagerShowKeyboard = "ShowKeyboard";
        private static readonly MethodInfo MethodToInputType;
        private static readonly MethodInfo MethodHideKeyboard;
        private static readonly MethodInfo MethodShowKeyboard;

        /// <summary>
        /// Initializes static members of the <see cref="KeyboardHelper" /> class.
        /// </summary>
        static KeyboardHelper()
        {
            var keyboardExtensionsType = Type.GetType(KeyboardExtensionsType, true);
            MethodToInputType = keyboardExtensionsType.GetMethod(KeyboardExtensionsToInputType);
            
            var keyboardManagerType = Type.GetType(KeyboardManagerType, true);
            MethodHideKeyboard = keyboardManagerType.GetMethod(KeyboardManagerHideKeyboard, BindingFlags.Static);
            MethodShowKeyboard = keyboardManagerType.GetMethod(KeyboardManagerShowKeyboard, BindingFlags.Static);
        }
        
        public static InputTypes GetInputType(Keyboard keyboard)
        {
            return (InputTypes)MethodToInputType.Invoke(null, new object[] { keyboard });
        }

        public static void HideKeyboard(View view)
        {
            try
            {
                InputMethodManager inputMethodManager = view.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
                inputMethodManager.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
            }
            catch(Exception ex)
            {
                // ignore
            }
        } 

        public static void ShowKeyboard(View view)
        {
            try
            {
                view.RequestFocus();
                InputMethodManager inputMethodManager = view.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
                inputMethodManager.ShowSoftInput(view, ShowFlags.Forced);
                inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly  );
            }
            catch(Exception ex)
            {
                // ignore
            }


        }
    }
}