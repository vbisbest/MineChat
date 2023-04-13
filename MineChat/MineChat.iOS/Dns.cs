//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MineChatAPI;
//using System.Net;
//using System.Threading.Tasks;
//using MonoTouch;
//using System.Runtime.InteropServices;
//using Xamarin;

//[assembly: Xamarin.Forms.Dependency(typeof(MineChat.iOS.Dns))]
//namespace MineChat.iOS
//{
//    public class Dns : IDNS
//    {
//        public event DnsLookupHandler OnDNSLookupComplete;
//        public event EventHandler OnDNSLookupCompleteDelegate;

//        public delegate void DnsLookupHandler(OnDNSLookupCompleteEventArgs args);

//        public Dns()
//        {
//        }            

//        private IPAddress ResolveIP(string address)
//        {
//            IPHostEntry ip = System.Net.Dns.GetHostEntry(address);

//            foreach(IPAddress ipAddress in ip.AddressList)
//            {
//                if(ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
//                {
//                    return ipAddress;
//                }
//            }

//            return IPAddress.None;
//        }            

//        //public void GetRealIPAndPort(Server server)
//        //{
//        //    Task task = Task.Run(() => InternalGetRealIPAndPort(server));
//        //}

//        public Task GetRealIPAndPort(Server server)
//        {
//            return Task.Run(() => InternalGetRealIPAndPort(server));
//        }

//        private void InternalGetRealIPAndPort(Server server) 
//        {
//            Dictionary<string, string> steps = new Dictionary<string, string>();
//            bool success = true;

//            try
//            {
                            
//                //Console.WriteLine("GetRealIPAndPort: " + server.Address);

//                steps.Add("Step1", "Start " + server.FullAddress);

//                IPAddress ip = null;
//                IPAddress ipOut;

//                steps.Add("Step5", "Begin parse address");

//                if (!IPAddress.TryParse(server.Address, out ipOut))
//                {
//                    steps.Add("Step6", "Using host name");
//                    steps.Add("Step7", "Begin resolve");
//                    //Response response;
//                    string query = "_minecraft._tcp." + server.Address + ".";

//                    SrvRecord[] response = null;

//                    try
//                    {
//                        response = SrvResolver.Resolve(query);
//                    }
//                    catch
//                    {
//                        // Ignore
//                    }

//                    if (response==null || response.Length == 0)
//                    {
//                        steps.Add("Step8", "No SRV record");

//                        // no service record, get the IP directly
//                        ip = ResolveIP(server.Address);

//                        server.RealIP = ip.ToString();
//                        server.RealPort = Convert.ToInt32(server.Port);

//                        steps.Add("Step9", "Complete");

//                        //return;
//                    }
//                    else
//                    {
//                        Console.WriteLine(query);

//                        //Console.WriteLine("got service record");
//                        // Got an srv record so grab that as the real info                            
//                        steps.Add("Step10", "Got " + response.Length.ToString() + " records");
//                        SrvRecord rec = response[0];

//                        steps.Add("Step11", "SRV record target = " + rec.Name);

//                        ip = ResolveIP(rec.Name);

//                        if (server.Port != "25565")
//                        {
//                            server.RealPort = Convert.ToInt32(server.Port);
//                        }
//                        else
//                        {
//                            server.RealPort = rec.Port;
//                        }

//                        if (ip == IPAddress.None)
//                        {
//                            throw new Exception("Could not resolve address");
//                        }

//                        steps.Add("Step14", "Read IP");
//                        server.RealIP = ip.ToString();
//                    }
//                }
//                else
//                {
//                    steps.Add("Step16", "Using IP address");
//                    server.RealIP = server.Address;
//                    server.RealPort = Convert.ToInt32(server.Port);
//                }
//            }
//            catch (Exception ex)
//            {
//                string message = ex.Message;

//                if (ex.StackTrace != string.Empty)
//                {
//                    steps.Add("Stack Trace", ex.StackTrace);
//                }

//                //Exception newException = new Exception("Error GetRealIPAndPort", ex);
//                //Insights.Report(newException, steps, Insights.Severity.Warning);
//                server.Status = ServerStatus.Error;
//                server.MOTD = ex.Message;

//                //foreach(KeyValuePair<string, string> s in steps)
//               // {
//                //    server.MOTD += "\r\n" + s.Value;
//                //}

//                success = false;
//            }
//            finally
//            {
//                if (OnDNSLookupComplete != null)
//                {
//                    OnDNSLookupCompleteEventArgs args = new OnDNSLookupCompleteEventArgs();
//                    args.Server = server;
//                    args.Success = success;
//                    OnDNSLookupComplete(args);
//                }
//            }
//        }

//    }
//}

