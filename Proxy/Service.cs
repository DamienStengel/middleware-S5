using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Proxy
{
    public class Service : IService
    {
        private const string BaseUrl = "https://api.jcdecaux.com/vls/v3/";
        private const string ApiKey = "f43a772a0e8818f54fa0f0c2628d4e2a2375b0c7";
        private List<Contract> contracts;
        private const double PROXY_REFRESH_TIME = 120;

        public async Task<JCDStation> GetNearestStation(Position position, bool isCheckingBikeAvailability)
        {
            Trace.WriteLine("GetNearestStation - Début");
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); 
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    if (contracts == null)
                    {
                        Trace.WriteLine("GetNearestStation - Chargement des contrats");
                        contracts = new List<Contract>();
                        var JCDContracts = await GetContracts(client);
                        foreach (var contract in JCDContracts)
                        {
                            var contractResult = new Contract
                            {
                                name = contract.name,
                                stations = await GetStationsForContract(client, contract.name),
                                lastRefresh = DateTime.Now
                            };
                            contracts.Add(contractResult);
                        }
                    }

                    var nearestContract = GetNearestContract(position);
                    Trace.WriteLine($"GetNearestStation - Contrat le plus proche : {nearestContract?.name}");

                    TimeSpan refreshTimeSpan = TimeSpan.FromSeconds(PROXY_REFRESH_TIME);
                    if (DateTime.Now - nearestContract.lastRefresh > refreshTimeSpan)
                    {
                        Trace.WriteLine($"GetNearestStation - Rafraîchissement des stations pour le contrat {nearestContract.name}");
                        nearestContract.stations = await GetStationsForContract(client, nearestContract.name);
                    }

                    var closestStation = FindClosestStation(position, nearestContract.stations, isCheckingBikeAvailability);
                    Trace.WriteLine($"GetNearestStation - Station la plus proche trouvée : {closestStation?.name}");
                    
                    stopwatch.Stop(); // Stop measuring time
                    TimeSpan elapsedTime = stopwatch.Elapsed;
                    Console.WriteLine($"Time to send station request: {elapsedTime.TotalMilliseconds} milliseconds");


                    return closestStation;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"GetNearestStation - Exception : {ex}");
                throw;
            }
        }

        private Contract GetNearestContract(Position currentPosition)
        {
            Trace.WriteLine("GetNearestContract - Début");
            try
            {
                Contract nearestContract = null;
                double nearestDistance = double.MaxValue;
                GeoCoordinate currentGeo = currentPosition.ToGeoCoordinate();

                foreach (var contract in contracts)
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
                    }
                }

                Trace.WriteLine($"GetNearestContract - Contrat le plus proche trouvé : {nearestContract?.name}");
                return nearestContract;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"GetNearestContract - Exception : {ex}");
                throw;
            }
        }

        static async Task<List<JCDContract>> GetContracts(HttpClient client)
        {
            Trace.WriteLine("GetContracts - Début");
            try
            {
                var response = await client.GetStringAsync($"contracts?apiKey={ApiKey}");
                var contracts = JsonSerializer.Deserialize<List<JCDContract>>(response);
                Trace.WriteLine("GetContracts - Contrats récupérés avec succès");
                return contracts;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"GetContracts - Exception : {ex}");
                throw;
            }
        }

        static async Task<List<JCDStation>> GetStationsForContract(HttpClient client, string contractName)
        {
            Trace.WriteLine($"GetStationsForContract - Début pour le contrat : {contractName}");
            try
            {
                var response = await client.GetStringAsync($"stations?contract={contractName}&apiKey={ApiKey}");
                var stations = JsonSerializer.Deserialize<List<JCDStation>>(response);
                Trace.WriteLine($"GetStationsForContract - Stations récupérées pour le contrat {contractName}");
                return stations;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"GetStationsForContract - Exception : {ex}");
                throw;
            }
        }

        static JCDStation FindClosestStation(Position target, List<JCDStation> stations, bool isCheckingForAvailability)
        {
            Trace.WriteLine("FindClosestStation - Début"+target.latitude+" "+target.longitude);
            try
            {
                GeoCoordinate targetCoord = target.ToGeoCoordinate();
                double closestDistance = double.MaxValue;
                JCDStation closestStation = null;

                foreach (var station in stations)
                {
                    GeoCoordinate stationCoord = station.position.ToGeoCoordinate();
                    double distance = targetCoord.GetDistanceTo(stationCoord);

                    if ((isCheckingForAvailability && station.mainStands.availabilities.bikes > 0) ||
                        (!isCheckingForAvailability && station.mainStands.availabilities.bikes < station.mainStands.availabilities.stands))
                    {
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestStation = station;
                        }
                    }
                }

                if (closestStation == null)
                {
                    Trace.WriteLine("FindClosestStation - Aucune station trouvée");
                    throw new Exception("No bike available in nearby station");
                }

                Trace.WriteLine($"FindClosestStation - Station la plus proche trouvée : {closestStation?.name}");
                return closestStation;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"FindClosestStation - Exception : {ex}");
                throw;
            }
        }
    }
}
