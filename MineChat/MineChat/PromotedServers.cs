using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineChat
{
    public class PromotedServers
    {
        public DateTime LastUpdate = DateTime.Now;
        public List<ServerWrapper> Servers = new System.Collections.Generic.List<ServerWrapper>();
    }
}
