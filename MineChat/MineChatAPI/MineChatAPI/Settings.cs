using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace MineChatAPI
{
    //[Serializable]
    [XmlRootAttribute("Settings")]
    public class Settings
    {
        public int Font = 0;
        public int FontSize = 3;
        public bool HideServerMessages = false;
        public bool SpawnOnConnect = true;
        public bool AutoCorrect = true;

        public bool AppRated = false;
        public int ConnectedCount = 0;

        public string CustomButton1 = string.Empty;
        public string CustomButton2 = string.Empty;
        public string CustomButton3 = string.Empty;
        public string LogonMessage = string.Empty;

        public bool ShowFeaturedServers = true;
        public bool FacebookAsked = false;
        public bool AlertVibrate = false;
        public bool AlertSound = false;

        private string alertWords = string.Empty;
        private List<string> alertWordsList = new List<string>();
        private List<Account> accountsList = new List<Account>();
        private List<Command> commandsList = new List<Command>();

        public bool AutoHideKeyboard = false;
        public int BackgroundImageOrientation = 0;

        public string settingsPath;
        public List<string> Friends = new List<string>();

        public bool IsLite = false;
        public DateTime LastFeaturedServerUpdate;

        List<Server> servers = new List<Server>();
        List<AlertWord> alerts = new List<AlertWord>();

        public List<int> SeenMessages = new List<int>();

        public bool ShowHeads { get; set; }

        public DateTime LastMessagesCheck { get; set; }

        [XmlIgnore]
        public List<string> AlertsList
        {
            get
            {
                return this.alertWordsList;
            }
            set
            {
                alertWordsList = value;
            }
        }

        public string AlertWords
        {
            get
            {
                return this.alertWords;
            }
            set
            {
                this.alertWords = value;
                this.alertWordsList.Clear();
                this.alertWordsList.AddRange(this.alertWords.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList());
            }
        }

        public Account GetSelectedAccount()
        {
            try
            {
                if (Accounts.Count == 0)
                {
                    throw new Exception("No accounts have been created");
                }

                Account account = accountsList.FirstOrDefault(a => a.Selected == true);
                if (account == null)
                {
                    throw new Exception("No accounts are selected");
                }

                return account;

            }
            catch(Exception ex)
            {
                return new Account();
            }

            
        }

        public Settings()
        {
            ShowHeads = true;
        }

        [XmlArray("Accounts")]
        [XmlArrayItem("Account", typeof(Account))]
        public List<Account> Accounts
        {
            get { return this.accountsList; }
            set { this.accountsList = value ?? new List<Account>(); }
        }

        [XmlArray("Commands")]
        [XmlArrayItem("Command", typeof(Command))]
        public List<Command> Commands
        {
            get { return this.commandsList; }
            set { this.commandsList = value ?? new List<Command>(); }
        }

        [XmlArray("Servers")]
        [XmlArrayItem("Server", typeof(Server))]
        public List<Server> Servers
        {
            get { return this.servers; }
            set { this.servers = value ?? new List<Server>(); }
        }

        [XmlArray("Alerts")]
        [XmlArrayItem("AlertWord", typeof(AlertWord))]
        public List<AlertWord> Alerts
        {
            get { return this.alerts; }
            set { this.alerts = value ?? new List<AlertWord>(); }
        }

        public static Settings LoadSettings(string settings)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
                TextReader reader = new StringReader(settings);

                Settings s = (Settings)deserializer.Deserialize(reader);
                return s;


            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return new Settings();
            }
        }

        public override string ToString()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }        
        }

        private bool Check(string serverName)
        {
            return (Servers.Exists(
                delegate(Server x) { return x.ServerName == serverName; }
            ));
        }
    }
}

