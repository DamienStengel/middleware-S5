using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServerApp.OpenRouteClasses
{
    public class Step
    {
        public string Instruction { get; set; }
        public double Distance { get; set; } // Distance in meters
        public double Duration { get; set; } // Duration in seconds
    }
}
