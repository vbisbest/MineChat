using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using MineChat;
using MineChat.iOS;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Foundation;

[assembly: ExportRenderer(typeof(ChatEntry), typeof(ChatEntryRenderer))]
namespace MineChat.iOS
{
    public class ChatEntryRenderer : EntryRenderer, IUITextFieldDelegate
    {
        private UIToolbar _inputAccessoryView;
        private ChatEntry XFchatEntry;

        List<UIBarButtonItem> buttons = new List<UIBarButtonItem>();

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            
            base.OnElementChanged(e);
            var chatEntry = this.Control as UITextField;


            if (e.NewElement == null)
            {
                if (chatEntry != null)
                {
                    chatEntry.EditingChanged -= ChatEntry_EditingChanged;
                }
                return;
            }

            if (Control != null && Control.InputAccessoryView == null)
            {
                XFchatEntry = (ChatEntry)e.NewElement;


                chatEntry.AutocapitalizationType = UITextAutocapitalizationType.None;
                chatEntry.ReturnKeyType = UIReturnKeyType.Send;
                chatEntry.EditingChanged += ChatEntry_EditingChanged;
                chatEntry.Delegate = this;


                ConfigureEntry();

                XFchatEntry.ConfigureEntryDelegate = () =>
                {
                    ConfigureEntry();
                };

                XFchatEntry.DetachEntryDelegate = () =>
                {
                    chatEntry.Delegate = null;
                };

            }

        }

        [Export("textFieldShouldReturn:")]
        public bool ShouldReturn(UITextField textField)
        {
			if (Global.CurrentSettings.AutoHideKeyboard)
			{
				textField.ResignFirstResponder();
			}

			XFchatEntry.SendChat();
			return true;
        }

        [Export("textFieldShouldBeginEditing:")]
        public bool ShouldBeginEditing(UITextField textField)
        {
            if (Global.IsConnected)
            {
                return true;
            }

            XFchatEntry.Tapped();
            return false;
        }

        private void ConfigureEntry()
        {
            if (Global.CurrentSettings.AutoCorrect && UIScreen.MainScreen.Bounds.Height > 480)
            {
                this.Control.AutocorrectionType = UITextAutocorrectionType.Yes;
            }
            else
            {
                this.Control.AutocorrectionType = UITextAutocorrectionType.No;
            }

            if (UIScreen.MainScreen.Bounds.Height > 480)
            {
                _inputAccessoryView = new UIToolbar();
                _inputAccessoryView.SizeToFit();

                this.Control.InputAccessoryView = _inputAccessoryView;
            }
            else
            {
                this.Control.InputAccessoryView = null;
            }
        }


        private void ChatEntry_EditingChanged(object sender, EventArgs e)
        {
            HandleChatValueChanged();
        }

        private void HandleChatValueChanged()
        {
            try
            {
                string[] words = Control.Text.Split(" ".ToCharArray());
                string currentWord = words[words.Length - 1];

                buttons = new List<UIBarButtonItem>();

                if (currentWord.StartsWith("/", StringComparison.CurrentCulture))
                {
                    List<MineChatAPI.Command> commands = Global.CurrentSettings.Commands.FindAll(c => c.Type == MineChatAPI.CommandType.Server);
                    IEnumerable sorted = commands.OrderBy(c => c.CommandText);

                    //UIBarButtonItem[] btnKBItems = new UIBarButtonItem[];
                    foreach (MineChatAPI.Command currentCommand in sorted)
                    {
                        UIBarButtonItem currentButton = new UIBarButtonItem(currentCommand.CommandText, UIBarButtonItemStyle.Plain, HandleToolbarButtonPressed);
                        buttons.Add(currentButton);
                    }
                }
                else if (currentWord.Length > 1)
                {
                    // Look for player names
                    foreach (Player player in Global.OnlinePlayers)
                    {
                        if (player.CleanName.StartsWith(currentWord, StringComparison.CurrentCultureIgnoreCase))
                        {
                            UIBarButtonItem currentButton = new UIBarButtonItem(player.CleanName, UIBarButtonItemStyle.Plain, HandleToolbarButtonPressed);
                            buttons.Add(currentButton);
                        }
                    }
                }
                else
                {
                    buttons.Clear();
                    HideInputAccessoryView();
                }

                if (buttons.Count > 0)
                {
                    _inputAccessoryView.Items = buttons.ToArray();
                    Control.ReloadInputViews();
                }
                else
                {
                    HideInputAccessoryView();
                }
            }
            catch (Exception ex)
            {
                HideInputAccessoryView();
            }
        }

        private void HideInputAccessoryView()
        {
            if (_inputAccessoryView != null && _inputAccessoryView.Items != null)
            {
                _inputAccessoryView.Items = null;
                if (Control != null)
                {
                    Control.ReloadInputViews();
                }
            }
        }

        private void HandleToolbarButtonPressed(object sender, EventArgs e)
        {

            UIBarButtonItem button = (UIBarButtonItem)sender;

            List<string> words = new List<string>();
            words.AddRange(Control.Text.Split(" ".ToCharArray()));

            words[words.Count - 1] = button.Title;

            string newSentance = string.Empty;

            foreach (string currentWord in words)
            {
                newSentance += currentWord + " ";
            }

            Control.Text = newSentance;
            XFchatEntry.Text = newSentance;

            HideInputAccessoryView();
        }
    }
}