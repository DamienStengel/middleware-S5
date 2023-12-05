using System.Collections.Generic;

namespace RoutingServerApp.OpenRouteClasses
{
    public class Geometry
    {
        public string type { get; set; }
        public List<List<double>> coordinates { get; set; }
    }
}