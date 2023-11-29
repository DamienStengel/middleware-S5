using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    internal class Contract
    {
        public string name { get; set; }
        public DateTime lastRefresh { get; set; }
        public List<JCDStation> stations { get; set; }
    }
}
