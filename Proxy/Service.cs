using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Proxy
{
    public class Service : IService
    {
        public JCDStation GetNearestStation(Position position, bool isCheckingBikeAvailability)
        {

            throw new NotImplementedException();
        }
        private const string BaseUrl = "https://api.jcdecaux.com/vls/v3/";
        private const string ApiKey = "f43a772a0e8818f54fa0f0c2628d4e2a2375b0c7";

        static async Task Main(string[] args)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);

                // List all contracts
                var contracts = await GetContracts(client);
                foreach (var contract in contracts)
                {
                    Console.WriteLine(contract.name);
                }

                Console.WriteLine("Choose a contract:");
                string chosenContract = Console.ReadLine();

                // Retrieve stations for the chosen contract
                var stations = await GetStationsForContract(client, chosenContract);
                foreach (var station in stations)
                {
                    Console.WriteLine(station.name);
                    Console.WriteLine(station.number);
                }

                Console.WriteLine("Choose a station:");
                int chosenStationNumber = int.Parse(Console.ReadLine());

                var chosenStation = stations.Find(s => s.number == chosenStationNumber);
                var closestStation = FindClosestStation(chosenStation, stations);

                Console.WriteLine($"The closest station to {chosenStationNumber} is {closestStation.number}");
                Console.ReadLine();
            }
        }

        static async Task<List<JCDContract>> GetContracts(HttpClient client)
        {
            var response = await client.GetStringAsync($"contracts?apiKey={ApiKey}");
            return JsonSerializer.Deserialize<List<JCDContract>>(response);
        }

        static async Task<List<JCDStation>> GetStationsForContract(HttpClient client, string contractName)
        {
            var response = await client.GetStringAsync($"stations?contract={contractName}&apiKey={ApiKey}");
            return JsonSerializer.Deserialize<List<JCDStation>>(response);
        }

        static JCDStation FindClosestStation(JCDStation target, List<JCDStation> stations)
        {
            GeoCoordinate targetCoord = new GeoCoordinate(target.position.latitude, target.position.longitude);
            double closestDistance = double.MaxValue;
            JCDStation closestStation = null;

            foreach (var station in stations)
            {
                if (station.number == target.number) continue;

                GeoCoordinate stationCoord = new GeoCoordinate(station.position.latitude, station.position.longitude);
                double distance = targetCoord.GetDistanceTo(stationCoord);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestStation = station;
                }
            }

            return closestStation;
        }
    }
}
}
