using System;
using System.Collections.Generic;
using Xamarin.Forms;
using MineChat;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using MineChat.Droid;
using Java.Lang;
using System.Diagnostics;
using System.ComponentModel;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Content;
using Android.Runtime;

[assembly: ExportRenderer(typeof(ChatEntry), typeof(ChatEntryRenderer))]
namespace MineChat.Droid
{
    class ChatEntryRenderer : ViewRenderer<ChatEntry, MultiAutoCompleteTextView>
    {
        MultiAutoCompleteTextView _chatEntry;
        ChatEntry _chat;
        string hash;
        //private OnTouchListener touchListener;

        protected override void OnElementChanged(ElementChangedEventArgs<ChatEntry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                return;
            }

            _chat = e.NewElement as ChatEntry;
            _chat.PropertyChanged += _chat_PropertyChanged;
            _chat.Focused += _chat_Focused;

            _chatEntry = new MultiAutoCompleteTextView(Forms.Context);
            SetNativeControl(_chatEntry);
            _chatEntry.SetTextColor(Android.Graphics.Color.Black);
            _chatEntry.SetHintTextColor(Android.Graphics.Color.LightGray);
            
            //_chatEntry.Hint =_chat.HintConnectToServer;
            _chatEntry.SetSingleLine(true);
            _chatEntry.ImeOptions = Android.Views.InputMethods.ImeAction.Send;
            _chatEntry.Touch += _chatEntry_Touch;
            _chatEntry.TextChanged += ChatEntry_TextChanged;



            InputMethodManager inputMethodManager = _chatEntry.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.HideSoftInputFromWindow(_chatEntry.WindowToken, HideSoftInputFlags.ImplicitOnly);

            if (Global.CurrentSettings.AutoCorrect)
            {
                _chatEntry.SetRawInputType(InputTypes.ClassText);
            }
            else
            {
                _chatEntry.SetRawInputType(InputTypes.TextFlagNoSuggestions);
            }

            _chatEntry.SetTokenizer(new SpaceTokenizer());
            _chatEntry.SetBackgroundColor(Android.Graphics.Color.White);
            _chatEntry.KeyPress += Control_KeyPress; ;
                
            _chat.HideKeyboardDelegate = () =>
            {
                KeyboardHelper.HideKeyboard(this.Control);
            };
        }

        private void _chat_Focused(object sender, FocusEventArgs e)
        {
            KeyboardHelper.ShowKeyboard(this);
        }

        private void _chatEntry_Touch(object sender, TouchEventArgs e)
        {
            e.Handled = false;

            try
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    if (!Global.IsConnected)
                    {
                        e.Handled = true;
                        _chat.Tapped();
                    }
                }
            }
            catch
            {
                // ignore
            }

        }

        protected override void Dispose(bool disposing)
        {
            _chat.PropertyChanged -= _chat_PropertyChanged;
            _chatEntry.Touch -= _chatEntry_Touch;
            _chatEntry.TextChanged -= ChatEntry_TextChanged;
            base.Dispose(disposing);
        }

        private void _chat_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Console.WriteLine(e.PropertyName);
            if(e.PropertyName == "Placeholder")
            {
                if (_chatEntry != null)
                {
                    _chatEntry.Hint = _chat.Placeholder;
                }
            }
        }

        private void Control_KeyPress(object sender, KeyEventArgs e)
        {
            e.Handled = false;

            try
            {
                if (e.Event.Action == null || e.Event.Action != KeyEventActions.Down)
                {
                    return;
                }

                if (e.KeyCode == Keycode.DpadLeft)
                {
                    _chat.HistoryBackward();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keycode.DpadRight)
                {
                    _chat.HistoryForward();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keycode.Enter)
                {
                    Console.WriteLine("Enter");

                    _chat.Text = _chatEntry.Text;
                    _chat.SendChat();
                    e.Handled = true;
                    if (Global.CurrentSettings.AutoHideKeyboard)
                    {
                        KeyboardHelper.HideKeyboard(this.Control);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Console.WriteLine(Control.IsInTouchMode);
            //Console.WriteLine(Control.Touchables);

            base.OnElementPropertyChanged(sender, e);
            //System.Diagnostics.Debug.WriteLine(e.PropertyName.ToString());
            if(this.Control == null || this.Element == null)
            {
                return;
            }

            if (e.PropertyName == Entry.TextProperty.PropertyName)
            {
                if (this.Control.Text != this.Element.Text)
                {
                    this.Control.Text = this.Element.Text;

                    this.Control.SetSelection(this.Control.Text.Length);                    
                    KeyboardHelper.ShowKeyboard(this.Control);
                }
            }            
        }

        public void ShowKeyboard()
        {
        }

        private void SetAdapter()
        {
            List<string> final = new List<string>();
            List<MineChatAPI.Command> commands = Global.CurrentSettings.Commands.FindAll(c => c.Type == MineChatAPI.CommandType.Server);

            foreach (MineChatAPI.Command command in commands)
            {
                final.Add(command.CommandText);
            }

            foreach (Player player in Global.OnlinePlayers)
            {
                final.Add(player.CleanName);
            }

            string tempHash = string.Empty;

            foreach(string s in final)
            {
                tempHash += s;
            }

            if (tempHash != hash)
            {
                ArrayAdapter adapter = new ArrayAdapter(Forms.Context, Android.Resource.Layout.SimpleDropDownItem1Line, final);
                _chatEntry.Adapter = adapter;
                hash = tempHash;
            }
        }

        private void ChatEntry_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            
            if(_chatEntry == null || _chatEntry.Text.Length < 2)
            {
                return;
            }

            SetAdapter();
            
        }
    }

    public class SpaceTokenizer : Java.Lang.Object, MultiAutoCompleteTextView.ITokenizer
    {
        public SpaceTokenizer()
        {
        }

        public int FindTokenEnd(ICharSequence text, int cursor)
        {
            return text.Length();
        }

        public int FindTokenStart(ICharSequence text, int cursor)
        {
            int i = cursor;

            while (i > 0 && text.CharAt(i - 1) != ' ')
            {
                i--;
            }
            while (i < cursor && text.CharAt(i) == ' ')
            {
                i++;
            }

            return i;
        }

        public ICharSequence TerminateTokenFormatted(ICharSequence text)
        {

            return text;
        }
    }
}