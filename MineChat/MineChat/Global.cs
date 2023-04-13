using System;
using Xamarin.Forms;
using System.IO;
using MineChatAPI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MineChat
{
    public class Global
    {        
        //public static List<InAppProduct> InAppProducts;
        //public static List<InAppPurchase> InAppPurchases;
        public static IPurchases InAppService;

        public static string PURCHASE_VIDEO = "promoted_video";

        public static string ANALYTICS_CONNECTION_ESTABLISHED = "Connection_Established";
        public static string ANALYTICS_FAILED_LOGIN = "Failed_Login";
        public static string ANALYTICS_VIDEO_PLAYED = "Video_Played";
        public static string ANALYTICS_LOG_ERROR = "Error";

        public static Settings CurrentSettings = null;
        public static List<string> Fonts = new List<string>() { "Verdana", "Helvetica", "Courier", "Times New Roman" };
        public static List<int> FontSizes = new List<int>() { 10, 12, 14, 16, 18, 20, 22 };

        public static bool IsOnGround = true;
        public static float WidthOffset = 2f;
        public static float XOffset = 2f;
        public static bool IsConnected = false;
        public static int CurrentProtocol = 0;

        public static string MineFriendsConnect = string.Empty;

        public static ObservableCollection<Player> OnlinePlayers = new ObservableCollection<Player>();
        //public static ObservableSortedList<Player> OnlinePlayers = new ObservableSortedList<Player>();
        public static int CurrentRealmsProtocol = 736;

        public static PromotedServers CurrentPromotedServers = new PromotedServers();
        public static MineChatAPI.MinecraftAPI MineChatAPI = null;
        public static DateTime ServerConnectTime;

        //public static int AD_INITIAL_SHOW = 60000;
        //public static int AD_REPEAT_SHOW = 60000;

        public static int AD_INITIAL_SHOW = 150000;
        public static int AD_REPEAT_SHOW = 800000;


        public static bool Appodeal_Initialized = false;
        public static bool FeaturedServersChanged = false;

        public static Dictionary<string, byte[]> SkinCache;
        public static List<string> CurrentPlayers;

        public static bool IsGooglePlay
        {
            get { return false; }
        }

        public static Account GetSelectedAccount()
        {
            return Global.CurrentSettings.Accounts.FirstOrDefault(a => a.Selected == true);
        }

        public static string AppodealKey
        {
            get
            {
                if(IsGooglePlay)
                {
                    // Google
                    return "6fa02ca2eb54d11211c262d927dea6ae338297b607c2e97f";
                }
                else
                {
                    // Amazon
                    return "e6173a920ab00ecfc59a1f2ab4580ea5e7feb46bcb5e211e";
                }
            }
       }


        public Global()
        {
            
        }

        public static void ScrollToWithDelay(ListView lv, object item, ScrollToPosition position, bool animated = true)
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
            {
                try
                {
                    lv.ScrollTo(item, position, animated);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return false;
            });
        }

        public static Color MainColor
        {
            get
            {
                return Color.FromHex("#1094df");
            }
        }

        public static ImageSource BytesToImageSource(byte[] bytes)
        {
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }

        public static void SaveSettings()
        {
            try
            {
                var storage = DependencyService.Get<IStorage>();
                storage.SaveFile("settings.xml", Global.CurrentSettings.ToString());
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static void LoadSettings()
        {
            try
            {
                var storage = DependencyService.Get<IStorage>();
                string temp = storage.LoadFile("settings.xml");
                Global.CurrentSettings = Settings.LoadSettings(temp);                          
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Global.CurrentSettings = new Settings();
            }
            finally
            {
                if (Global.CurrentSettings.Commands.Count == 0)
                {
                    MineChatAPI.Command command = new MineChatAPI.Command();
                    command.CommandText = "/msg";
                    command.Type = CommandType.Player;
                    Global.CurrentSettings.Commands.Add(command);

                    MineChatAPI.Command command2 = new MineChatAPI.Command();
                    command2.CommandText = "/spawn";
                    command2.Type = CommandType.Server;
                    Global.CurrentSettings.Commands.Add(command2);
                }
            }
            
        }

        public static string MinecraftStringToPlain(string message)
        {
            message = message.Replace("§c", string.Empty);
            message = message.Replace("§4", string.Empty);
            message = message.Replace("§3", string.Empty);
            message = message.Replace("§f", string.Empty);
            message = message.Replace("§0", string.Empty); // This is black, make it gray to be visible
            message = message.Replace("§2", string.Empty);
            message = message.Replace("§1", string.Empty);
            message = message.Replace("§5", string.Empty);
            message = message.Replace("§6", string.Empty);
            message = message.Replace("§7", string.Empty);
            message = message.Replace("§8", string.Empty);
            message = message.Replace("§9", string.Empty);
            message = message.Replace("§a", string.Empty);
            message = message.Replace("§b", string.Empty);
            message = message.Replace("§d", string.Empty);
            message = message.Replace("§e", string.Empty);
            message = message.Replace("§k", string.Empty);
            message = message.Replace("§o", string.Empty);
            message = message.Replace("§m", string.Empty);
            message = message.Replace("§n", string.Empty);
            // Need to handle
            message = message.Replace("§r", string.Empty);
            message = message.Replace("§l", string.Empty);
            message = message.Replace("  ", " ");
            return message;
        }

        public static FormattedString MinecraftStringToFormattedString(string message, int fontSize, string fontFamily)
        {

            if(message == string.Empty)
            {
                return null;
            }

            //"§4§l<3§r §r§b[Patron] coldtim§r For what"
            message.Replace("§k", string.Empty);

            if (!message.StartsWith("§"))
            {
                message = "§r" + message;
            }

            string currentChar = string.Empty;
            Span span = new Span();

            FormattedString fs = new FormattedString();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(message));
            StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                char c = (char)reader.Read();

                if (c.ToString () == "§")
                {
                    if (span.Text != null)
                    {
                        span.FontSize = fontSize;
                        if (fontFamily != string.Empty)
                        {
                            span.FontFamily = fontFamily;
                        }
                        fs.Spans.Add(span);
                        span = new Span();
                    }

                    AddSpanStyle(span, (char)reader.Read());
                    continue;
                }
                else
                {
                    span.Text += c;
                }
            }

            // Add the last span
            span.FontSize = fontSize;
            if (fontFamily != string.Empty)
            {
                span.FontFamily = fontFamily;
            }

            if (span.Text != null)
            {
                fs.Spans.Add(span);
            }

            return fs;
        }

        private static void AddSpanStyle(Span span, char code)
        {
            switch (code.ToString())
            {
                case "a":
                    span.ForegroundColor = Color.FromHex("#55ff55");
                    break;
                case "b":
                    span.ForegroundColor = Color.FromHex("#55ffff");
                    break;
                case "c":
                    span.ForegroundColor = Color.FromHex("#ff5555");
                    break;
                case "d":
                    span.ForegroundColor = Color.FromHex("#ff55ff");
                    break;
                case "e":
                    span.ForegroundColor = Color.FromHex("#ffff55");
                    break;
                case "f":
                    span.ForegroundColor = Color.FromHex("#ffffff");
                    break;
                case "0":
                    span.ForegroundColor = Color.FromHex("#aaaaaa");
                    break;
                case "1":
                    span.ForegroundColor = Color.FromHex("#0000aa");
                    break;
                case "2":
                    span.ForegroundColor = Color.FromHex("#00aa00");
                    break;
                case "3":
                    span.ForegroundColor = Color.FromHex("#00aaaa");
                    break;
                case "4":
                    span.ForegroundColor = Color.FromHex("#aa0000");
                    break;
                case "5":
                    span.ForegroundColor = Color.FromHex("#aa00aa");
                    break;
                case "6":
                    span.ForegroundColor = Color.FromHex("#ffaa00");
                    break;
                case "7":
                    span.ForegroundColor = Color.FromHex("#aaaaaa");
                    break;
                case "8":
                    span.ForegroundColor = Color.FromHex("#555555");
                    break;
                case "9":
                    span.ForegroundColor = Color.FromHex("#5555ff");
                    break;
                case "l":
                    span.FontAttributes = FontAttributes.Bold;
                    break;
                case "o":
                    span.FontAttributes = FontAttributes.Italic;
                    break;
                case "n":
                    break;
                case "m":
                    break;
                case "r":
                    span.FontAttributes = FontAttributes.None;
                    span.ForegroundColor = Color.White;
                    break;
                default:
                    //span.Foreground = HexColorToBrush("#ffffff");
                    break;
            }
        }

        /*
        public static void AuthenticateAccount(Account account)
        {
            if (account == null)
            {
                return;
            }

            MojangAPIV2.Validate(account.AccessToken, account.ClientToken).ContinueWith(task =>
            {
                if (task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                {
                    if (task.Result.Result == MojangPCL.RequestResult.Fail)
                    {
                        Debug.WriteLine("Validate failed, get new token");
                        MojangAPIV2.Authenticate(account.UserName, account.Password, account.ClientToken).ContinueWith(authTask =>
                        {
                            if (authTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                            {
                                if (authTask.Result.Result == MojangPCL.RequestResult.Success)
                                {
                                    account.AccessToken = authTask.Result.accessToken;
                                    account.ProfileID = authTask.Result.selectedProfile.id;
                                    account.PlayerName = authTask.Result.selectedProfile.name;
                                    account.ClientToken = authTask.Result.clientToken;
                                    Global.SaveSettings();
                                }
                                else
                                {
                                    throw new Exception(authTask.Result.Message);
                                }
                            }
                        }).Wait();
                    }
                }
                else
                {
                    Debug.WriteLine("Validate Issue");
                }
            }).Wait();            
        }
        */
    }
}

