using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServerApp.OpenRouteClasses
{
    public class Route
    {
        public Summary Summary { get; set; }
        public List<Segment> Segments { get; set; }
    }
}
