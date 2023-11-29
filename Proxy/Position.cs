using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    public class Position
    {
        public Double latitude { get; set; }
        public Double longitude { get; set; }
        public GeoCoordinate ToGeoCoordinate()
        {
            return new GeoCoordinate(latitude, longitude);
        }
    }
    
}
