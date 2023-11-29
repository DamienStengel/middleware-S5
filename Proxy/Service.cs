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
        private const string BaseUrl = "https://api.jcdecaux.com/vls/v3/";
        private const string ApiKey = "f43a772a0e8818f54fa0f0c2628d4e2a2375b0c7";
        private List<Contract> contracts;
        //Delay between on stations in second
        private const double PROXY_REFRESH_TIME = 120;
        public async Task<JCDStation> GetNearestStation(Position position, bool isCheckingBikeAvailability)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                if(contracts == null)
                {
                    var JCDContracts = await GetContracts(client);
                    foreach(var contract in JCDContracts)
                    {
                        var contractResult = new Contract();
                        contractResult.name = contract.name;
                        var JCDStations = await GetStationsForContract(client, contract.name);
                        contractResult.stations = JCDStations;
                        contractResult.lastRefresh = DateTime.Now;
                        this.contracts.Add(contractResult);
                    }
                }
                var nearestContract = getNearestContract(position);
                TimeSpan refreshTimeSpan = TimeSpan.FromSeconds(PROXY_REFRESH_TIME);
                if (DateTime.Now - nearestContract.lastRefresh < refreshTimeSpan)
                {
                    nearestContract.stations = await GetStationsForContract(client,nearestContract.name);

                }
                var closestStation = FindClosestStation(position, nearestContract.stations, isCheckingBikeAvailability);

                return closestStation;
            }
        }

        private Contract getNearestContract(Position currentPosition)
        {
            Contract nearestContract = null;
            double nearestDistance = double.MaxValue;
            GeoCoordinate currentGeo = currentPosition.ToGeoCoordinate();

            foreach (var contract in this.contracts)
            {
                foreach (var station in contract.stations)
                {
                    GeoCoordinate stationGeo = station.position.ToGeoCoordinate();
                    double distance = currentGeo.GetDistanceTo(stationGeo);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestContract = contract;
                    }
                    else
                    {
                        if(distance-nearestDistance < 100000)
                        {
                            break;
                        }
                    }
                }
            }

            return nearestContract;
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

        static JCDStation FindClosestStation(Position target, List<JCDStation> stations, Boolean isCheckingForAvailability)
        {
            GeoCoordinate targetCoord = target.ToGeoCoordinate();
            double closestDistance = double.MaxValue;
            JCDStation closestStation = null;

            foreach (var station in stations)
            {
                if (station.mainStands.availabilities.bikes > 0)
                {
                    GeoCoordinate stationCoord = station.position.ToGeoCoordinate();
                    double distance = targetCoord.GetDistanceTo(stationCoord);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestStation = station;
                    }
                }
            }
            if(closestStation == null)
            {
                throw new Exception("No bike available in nearby station");
            }

            return closestStation;
        }
    }
}
}
