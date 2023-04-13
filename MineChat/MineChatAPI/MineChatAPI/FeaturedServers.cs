using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections;
using System.Threading.Tasks;

namespace MineChatAPI
{
    public class FeaturedServers
    {
        public delegate void FeaturedServersReceivedHandler(List<Server> featuredServers, string error);
        public event FeaturedServersReceivedHandler FeaturedServersReceived;

        public delegate void UpdateFeaturedServersCompleteHandler(string error);
        public event UpdateFeaturedServersCompleteHandler UpdateFeaturedServersComplete;

        public delegate void FeaturedMessagesReceivedHandler(MessagesList messages, string error);
        public event FeaturedMessagesReceivedHandler MessagesReceived;


        private const string host = "redacted";

        public FeaturedServers()
        {
        }

        public void UpdateFeaturedServers(int serverId)
        {
            string error = string.Empty;
            Uri uri = new Uri(host + "redacted" + serverId);
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0,0,10);
            client.GetStringAsync(uri).ContinueWith(getTask =>
            {
                try
                {
                    string result = getTask.Result.ToString();

                    if (UpdateFeaturedServersComplete != null)
                    {
                        UpdateFeaturedServersComplete(string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    if (UpdateFeaturedServersComplete != null)
                    {
                        UpdateFeaturedServersComplete(ex.Message);
                    }
                }
            });                    
        }

        public void GetFeaturedServers()
        {
            string error = string.Empty;
            Uri uri = new Uri(host + "redacted");
            HttpClient client = new HttpClient();

            try
            {
                client.Timeout = new TimeSpan(0,0,10);

                client.GetStringAsync(uri).ContinueWith(getTask =>
                {
                    try
                    {
                        string result = getTask.Result.ToString();
                        var featuredServers = JsonConvert.DeserializeObject<PromotedServersList>(result);

                        List<Server> servers = new List<Server>();
                        foreach(PromotedServer currentServer in featuredServers.PromotedServers)
                        {
                            Server s = new Server();
                            s.FullAddress = currentServer.Host.Trim();
                            s.ServerName = currentServer.Name.Trim();
                            s.PromotedServerID = currentServer.PromotedServerID;

                            if(Convert.ToBoolean(currentServer.IsFeatured))
                            {
                                s.ServerType = ServerTypeEnum.Featured;
                            }
                            else
                            {
                                s.ServerType = ServerTypeEnum.Promoted;
                            }

                            servers.Add(s);
                        }
                            
                        if(FeaturedServersReceived != null)
                        {
                            FeaturedServersReceived(servers, error);
                        }
                    }
                    catch(AggregateException ae)
                    {
                        Debug.WriteLine(ae.InnerException.Message);

                        if(FeaturedServersReceived != null)
                        {
                            FeaturedServersReceived(null, "Could not load featured servers: " + ae.InnerException.Message);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        if(FeaturedServersReceived != null)
                        {
                            FeaturedServersReceived(null, "Could not load featured servers: " + e.Message);
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                if(FeaturedServersReceived != null)
                {
                    FeaturedServersReceived(null, "Could not load featured servers: " + ex.Message);
                }
            }
        }

        public static async Task<List<Server>> GetFeaturedServersAsync()
        {
            string error = string.Empty;
            Uri uri = new Uri(host + "redacted");
            HttpClient client = new HttpClient();

            client.Timeout = new TimeSpan(0, 0, 10);

            string result = await client.GetStringAsync(uri);
            
            var featuredServers = JsonConvert.DeserializeObject<PromotedServersList>(result);
            List<Server> servers = new List<Server>();
            foreach (PromotedServer currentServer in featuredServers.PromotedServers)
            {
                Server s = new Server();
                s.FullAddress = currentServer.Host.Trim();
                s.ServerName = currentServer.Name.Trim();
                s.PromotedServerID = currentServer.PromotedServerID;

                if (Convert.ToBoolean(currentServer.IsFeatured))
                {
                    s.ServerType = ServerTypeEnum.Featured;
                }
                else
                {
                    s.ServerType = ServerTypeEnum.Promoted;
                }

                servers.Add(s);
            }

            return servers;
        }


        public void GetMessages(PlatformID platformID)
        {
            string error = string.Empty;
            Uri uri = new Uri(host + "redacted" + (int)platformID);
            HttpClient client = new HttpClient();

            try
            {
                client.Timeout = new TimeSpan(0,0,10);
                client.GetStringAsync(uri).ContinueWith(getTask =>
                {
                    try
                    {
                        string result = getTask.Result.ToString();
                        var messages = JsonConvert.DeserializeObject<MessagesList>(result);
                        if (messages.Messages == null) 
                        {
                            messages.Messages = new List<Message> ();
                        }

                        if(MessagesReceived != null)
                        {
                            MessagesReceived(messages, string.Empty);
                        }
                    }
                    catch(AggregateException ae)
                    {
                        Debug.WriteLine(ae.InnerException.Message);

                        if(MessagesReceived != null)
                        {
                            MessagesReceived(null, "Could not load messages: " + ae.InnerException.Message);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        if(MessagesReceived != null)
                        {
                            MessagesReceived(null, "Could not load messages: " + e.Message);
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                if(MessagesReceived != null)
                {
                    MessagesReceived(null, "Could not load messages: " + ex.Message);
                }
            }
        }
    }

    public class PromotedServersList
    {
        public List<PromotedServer> PromotedServers { get; set; } 
    }

    public class MessagesList
    {
        public MessagesList ()
        {
            Messages = new List<Message>();
        }

        public List<Message> Messages { get; set; } 
    }

    public class PromotedServer
    {
        public int PromotedServerID { get; set; }
        public string Host { get; set; }
        public string Name { get; set; }
        public int IsFeatured { get; set; }
    }

    public class Message
    {
        public int MessageID { get; set; }
        public string Text { get; set; }
        public string Action { get; set; }
    }

    public enum PlatformID
    {
        iOS = 10,
        Android = 20,
        Windows = 30
    }

}

