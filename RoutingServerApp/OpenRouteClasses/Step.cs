namespace RoutingServerApp.OpenRouteClasses
{
    public class Step
    {
        public string instruction { get; set; }
        public double distance { get; set; } // Distance in meters
        public double duration { get; set; } // Duration in seconds
        public int[] way_points { get; set; }
    }
}
