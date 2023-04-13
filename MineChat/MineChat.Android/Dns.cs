//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Android.App;
//using Android.Content;
//using MineChatAPI;
//using System.Threading.Tasks;
//using Android.Net;
//using System.Net;
//using Android.Net.Wifi;
////using Heijden.DNS;
//using System.Net.Sockets;
//using Heijden.Dns.Portable;

//[assembly: Xamarin.Forms.Dependency(typeof(MineChat.Droid.Dns))]
//namespace MineChat.Droid
//{
//    public class Dns : IDNS
//    {
//        public event EventHandler OnDNSLookupCompleteDelegate;

//        public Dns()
//        {
//        }

//        public Task GetRealIPAndPort(Server server)
//        {
//            //    if(!server.FullAddress.Contains("boost"))
//            //    {
//            //        //return;
//            //    }

//            //    return Task.Run(() => InternalGetRealIPAndPort(server));
//            return null;
//        }

//        public IPAddress ResolveIP(string address)
//        {
//            try
//            {
//                IPHostEntry ip = System.Net.Dns.GetHostEntry(address);
//                foreach (IPAddress addr in ip.AddressList)
//                {
//                    if (addr.AddressFamily == AddressFamily.InterNetwork)
//                    {
//                        return addr;
//                    }
//                }

//                throw new Exception("Could not get IPV4 address");

//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Could not resolve IP address", ex);
//            }
//        }

//        public string[] GetDNSServer(Context context)
//        {
//            string dnsServer1 = string.Empty;
//            string dnsServer2 = string.Empty;

//            try
//            {
//                if (Connectivity.IsConnectedWifi(context))
//                {
//                    WifiManager wifiManager = (Android.Net.Wifi.WifiManager)context.GetSystemService(Service.WifiService);

//                    if (wifiManager == null)
//                    {
//                        throw new Exception("Could not get WifiManager service");
//                    }

//                    DhcpInfo d = wifiManager.DhcpInfo;
//                    if (d != null)
//                    {

//                        if (d.Dns1 != 0)
//                        {
//                            dnsServer1 = new IPAddress(d.Dns1).ToString();
//                        }
//                        else
//                        {
//                            dnsServer1 = "8.8.8.8";
//                        }

//                        if (d.Dns2 != 0)
//                        {
//                            dnsServer2 = new IPAddress(d.Dns2).ToString();
//                        }
//                        else
//                        {
//                            dnsServer2 = "8.8.4.4";
//                        }
//                    }
//                    else
//                    {
//                        throw new Exception();
//                    }
//                }
//                else
//                {
//                    throw new Exception();
//                }
//            }
//            catch
//            {
//                // just use google dns
//                dnsServer1 = "8.8.8.8";
//                dnsServer2 = "8.8.4.4";
//            }

//            return new string[] { dnsServer1, dnsServer2 };
//        }

//        //private void InternalGetRealIPAndPort(Server server)
//        //{
//        //    bool success = true;

//        //    Dictionary<string, string> steps = new Dictionary<string, string>();
//        //    steps.Add("Step1", "Start " + server.FullAddress);

//        //    IPAddress ip = null;
//        //    IPAddress ipOut;
//        //    string[] dnsServers = new string[0];

//        //    try
//        //    {
//        //        steps.Add("Step2", "Get DNS server");
//        //        dnsServers = GetDNSServer(Application.Context);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine(ex.Message);
//        //    }

//        //    if (dnsServers[0] == "8.8.8.8")
//        //    {
//        //        steps.Add("Step3", "Using Google DNS");
//        //    }
//        //    else
//        //    {
//        //        steps.Add("Step4", "Using local DNS");
//        //    }

//        //    try
//        //    {
//        //        steps.Add("Step5", "Begin parse address");

//        //        if ((server.Port == "0" || server.Port == "25565") && !IPAddress.TryParse(server.Address, out ipOut))
//        //        {
//        //            steps.Add("Step6", "Using host name");

//        //            Dealing with a host name
//        //           IPEndPoint[] ds = null;
//        //            ds = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse(dnsServers[0]), 53), new IPEndPoint(IPAddress.Parse(dnsServers[1]), 53) };

//        //            Resolver resolver = new Resolver(ds);
//        //            resolver.Timeout = new TimeSpan(3000);
//        //            resolver.Retries = 3;
//        //            resolver.UseCache = true;
//        //            resolver.TransportType = Heijden.DNS.TransportType.Udp;
//        //            resolver.Recursion = true;

//        //            steps.Add("Step7", "Begin resolve");
//        //            Response response;
//        //            response = resolver.Query("_minecraft._tcp." + server.Address + ".", QType.SRV).Result;

//        //            if (response.Error != string.Empty)
//        //            {
//        //                Resolve failure
//        //                steps.Add("DNS failure", response.Error);
//        //                steps.Add("DNS1", dnsServers[0]);
//        //                steps.Add("DNS2", dnsServers[1]);

//        //                throw new Exception("Error reaching DNS server");
//        //            }

//        //            if (response.Answers.Count() == 0)
//        //            {
//        //                steps.Add("Step8", "No SRV record");

//        //                no service record, get the IP directly
//        //                ip = ResolveIP(server.Address);


//        //                if (ip == null)
//        //                {
//        //                    server.RealIP = IPAddress.None.ToString();
//        //                }
//        //                else
//        //                {
//        //                    server.RealIP = ip.ToString();
//        //                }

//        //                server.RealPort = Convert.ToInt32(server.Port);
//        //                steps.Add("Step9", "Complete");

//        //                return;
//        //            }
//        //            else
//        //            {
//        //                Console.WriteLine("got service record");
//        //                Got an srv record so grab that as the real info
//        //                steps.Add("Step10", "Got " + response.Answers.Count.ToString() + " records");
//        //                AnswerRR rec = response.Answers[0];

//        //                if (rec.Type == DnsEntryType.SRV)
//        //                {

//        //                    RecordSRV recordSRV = (RecordSRV)rec.RECORD;
//        //                    steps.Add("Step11", "SRV record target = " + recordSRV.TARGET);

//        //                    ip = ResolveIP(recordSRV.TARGET);

//        //                    if (server.Port != "25565")
//        //                    {
//        //                        server.RealPort = Convert.ToInt32(server.Port);
//        //                    }
//        //                    else
//        //                    {
//        //                        server.RealPort = recordSRV.PORT;
//        //                    }

//        //                }
//        //                else if (rec.Type == DnsEntryType.CNAME)
//        //                {
//        //                    RecordCNAME recordCNAME = (RecordCNAME)rec.RECORD;
//        //                    steps.Add("Step12", "CNAME record target = " + recordCNAME.CNAME);

//        //                    if (recordCNAME.CNAME.Contains("_minecraft"))
//        //                    {
//        //                        string newHost = recordCNAME.CNAME.Replace("_minecraft._tcp.", "");
//        //                        if (newHost.EndsWith("."))
//        //                        {
//        //                            newHost = newHost.TrimEnd(".".ToCharArray());
//        //                        }

//        //                        ip = ResolveIP(newHost);
//        //                    }
//        //                    else
//        //                    {
//        //                        ip = ResolveIP(recordCNAME.CNAME);
//        //                    }

//        //                    server.RealPort = Convert.ToInt32(server.Port);

//        //                }
//        //                else if (rec.Type == DnsEntryType.A)
//        //                {

//        //                    RecordA recordA = (RecordA)rec.RECORD;
//        //                    steps.Add("Step17", "A record target = " + recordA.Address.ToString());
//        //                    ip = ResolveIP(recordA.Address.ToString());
//        //                }
//        //                else
//        //                {
//        //                    steps.Add("Step13", "Unknown record record = " + rec.Type.ToString());
//        //                    throw new Exception("Unknown DNS Record Type");
//        //                }

//        //                if (ip == null)
//        //                {
//        //                    throw new Exception("Could not resolve address");
//        //                }

//        //                steps.Add("Step14", "Read IP");
//        //                server.RealIP = ip.ToString();

//        //                if (server.RealIP.ToString() == "67.215.65.132")
//        //                {
//        //                    steps.Add("Step15", "Google nout found entry");
//        //                    This is a googleDNS not found entry.Try to use the host name
//        //                    IPAddress temp;
//        //                    string hostIP = server.Address;
//        //                    if (hostIP.EndsWith("."))
//        //                    {
//        //                        hostIP = hostIP.Substring(0, hostIP.Length - 1);
//        //                    }

//        //                    if (IPAddress.TryParse(hostIP, out temp))
//        //                    {
//        //                        server.RealIP = temp.ToString();
//        //                    }
//        //                    else
//        //                    {
//        //                        throw new Exception("Host name not found on Google DNS");
//        //                    }
//        //                }
//        //            }
//        //        }
//        //        else if (!IPAddress.TryParse(server.Address, out ipOut))
//        //        {
//        //            if (server.Port != "0")
//        //            {
//        //                port is specified, ignore anything else, just go with it
//        //               ip = ResolveIP(server.Address);
//        //                server.RealIP = ip.ToString();
//        //                server.RealPort = Convert.ToInt32(server.Port);
//        //            }
//        //        }
//        //        else
//        //        {
//        //            steps.Add("Step16", "Using IP address");
//        //            server.RealIP = server.Address;
//        //            server.RealPort = Convert.ToInt32(server.Port);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        string message = ex.Message;

//        //        if (ex.StackTrace != string.Empty)
//        //        {
//        //            steps.Add("Stack Trace", ex.StackTrace);
//        //        }

//        //        Exception newException = new Exception("Error GetRealIPAndPort", ex);

//        //        server.Status = ServerStatus.Error;
//        //        server.MOTD = ex.Message;
//        //        success = false;
//        //        throw newException;
//        //    }
//        //    finally
//        //    {
//        //        if (OnDNSLookupComplete != null)
//        //        {
//        //            OnDNSLookupCompleteEventArgs args = new OnDNSLookupCompleteEventArgs();
//        //            args.Server = server;
//        //            args.Success = success;
//        //            OnDNSLookupComplete(this, args);
//        //        }
//        //    }

//        //}
//    }
//}

