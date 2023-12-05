using System;
using System.ServiceModel;
using System.Threading.Tasks;
using TestWCF.Service;

namespace TestWCF
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Position position = new Position();
            position.longitude = 43.59585493021408;
            position.longitude = 1.4452020695219097;
            JCDStation nearestStation = getNearestStation(position, true).Result;
            Console.WriteLine(nearestStation.name);
        }
        private static async Task<JCDStation> getNearestStation(Position position, bool isInOriginMode)
        {
            var binding = new WSHttpBinding();
            var endpoint = new EndpointAddress("http://localhost:8090/Proxy/");

            var serviceClient = new ServiceClient(binding,endpoint);
            try
            {
                JCDStation nearestStation = await serviceClient.GetNearestStationAsync(position, isInOriginMode);

                // Use the nearestStation object as needed
                Console.WriteLine($"Nearest Station: {nearestStation.name}");
                return nearestStation;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw ex;
            }
        }
    }
}