using System;
using System.Collections.Generic;
using System.Text;
using MineChatAPI;
using DnsClient;
using System.Diagnostics;
using System.Linq;
using DnsClient.Protocol;
using System.Threading.Tasks;
using System.Net;
using Xamarin.Forms;

namespace MineChat
{
    public class DNS
    {
        LookupClient client;
        List<IPEndPoint> dnsServers = new List<IPEndPoint>();

        public DNS()
        {

        }

        public void GetDNSServers()
        {
            try
            {
                List<IPEndPoint> addresses = null;
                INetworkHelper device = DependencyService.Get<INetworkHelper>();
                dnsServers.Clear();

                if (device != null)
                {
                    addresses = device.GetDnsServers();

                    if (addresses != null)
                    {
                        foreach (IPEndPoint currentEndPoint in addresses)
                        {
                            if (!dnsServers.Contains(currentEndPoint))
                            {
                                dnsServers.Add(currentEndPoint);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // ignore
                Console.WriteLine(ex.Message);
            }

            // Add google servers
            try
            {
                var nameServers = NameServer.ResolveNameServers(false, true);
                if (nameServers != null)
                {
                    foreach (IPEndPoint endPoint in nameServers)
                    {
                        if (!dnsServers.Contains(endPoint))
                        {
                            dnsServers.Add(endPoint);
                        }
                    }
                }


                if (!dnsServers.Contains(NameServer.GooglePublicDns))
                {
                    dnsServers.Add(NameServer.GooglePublicDns);
                }

                if (!dnsServers.Contains(NameServer.GooglePublicDns2))
                {
                    dnsServers.Add(NameServer.GooglePublicDns);
                }

                foreach (IPEndPoint ip in dnsServers)
                {
                    Console.WriteLine(ip.Address.ToString());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            client = new LookupClient(dnsServers.ToArray());
            client.EnableAuditTrail = false;
            client.UseCache = true;
        }

        public async Task<Server> DoLookup(Server server)
        {
            GetDNSServers();

            if (!server.Address.ToLower().Contains("radial"))
            {
                //return;
            }

            OnDNSLookupCompleteEventArgs args = new OnDNSLookupCompleteEventArgs();
            args.Server = server;



            try
            {
                // check for ip address
                IPAddress ipOut = null;
                if (IPAddress.TryParse(server.Address, out ipOut))
                {
                    // Its an IP address
                    server.RealIP = server.Address;
                    server.RealPort = Convert.ToInt32(server.Port);
                    args.Success = true;
                }
                else
                {

                    var result = await client.QueryAsync("_minecraft._tcp." + server.Address + ".", QueryType.SRV);

                    if (result.AuditTrail != null)
                    {
                        Console.WriteLine(result.AuditTrail);
                    }

                    if (result.HasError || result.Answers.Count == 0)
                    {
                        try
                        {
                            server.RealIP = GetIPAddress(server.Address);
                            server.RealPort = Convert.ToInt32(server.Port);
                            args.Success = true;
                        }
                        catch (Exception ex)
                        {
                            server.Status = ServerStatus.Error;
                            server.MOTD = ex.Message;
                            args.Success = false;
                        }
                    }
                    else
                    {
                        // Dealing with a service record
                        var srvRecord = result.Answers.OfType<SrvRecord>().FirstOrDefault();

                        if (srvRecord != null)
                        {

                            var additionalRecord = result.Additionals.FirstOrDefault(p => p.DomainName.Equals(srvRecord.Target));

                            if (additionalRecord is ARecord aRecord)
                            {
                                Console.WriteLine($"Services found at {srvRecord.Target}:{srvRecord.Port} IP: {aRecord.Address}");
                            }
                            else if (additionalRecord is CNameRecord cname)
                            {
                                Console.WriteLine($"Services found at {srvRecord.Target}:{srvRecord.Port} IP: {cname.CanonicalName}");
                            }

                            try
                            {
                                server.RealIP = GetIPAddress(srvRecord.Target);
                                server.RealPort = srvRecord.Port;
                                args.Success = true;
                            }
                            catch (Exception exe)
                            {
                                server.Status = ServerStatus.Error;
                                server.MOTD = exe.Message;
                                args.Success = false;
                            }
                        }
                    }
                }

                return server;
            }
            catch(Exception ex)
            {
                server.Status = ServerStatus.Error;
                server.MOTD = ex.GetBaseException().Message + ": contact MineChat support";
                args.Success = false;
                return server;
            }
        }

        private string GetIPAddress(string address)
        {
            var result = client.Query(address, QueryType.A, QueryClass.IN);
            if(result.HasError || result.Answers.Count == 0)
            {
                throw new Exception("Could not resolve server address " + address);
            }

            var aRecord = result.Answers.OfType<ARecord>().FirstOrDefault();

            if(result.HasError)
            {
                throw new Exception(result.ErrorMessage);
            }

            return aRecord.Address.ToString();
        }
    }

    public class OnDNSLookupCompleteEventArgs
    {
        public bool Success { get; set; }
        public Server Server { get; set; }
    }
}
