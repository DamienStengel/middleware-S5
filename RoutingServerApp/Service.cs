using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Collections.Generic;
using RoutingServerApp.OpenRouteClasses;
using System.Diagnostics;
using System.ServiceModel;
using RoutingServerApp.Service;

namespace RoutingServer
{   public class Service : IService
    {
        const string OpenRouteServiceApiKey = "5b3ce3597851110001cf6248fad8725cf09348fa913d6e0ded7dfaf5";
        const string MODE_WALKING = "foot-walking";
        const string MODE_BIKING = "cycling-regular";
        public List<Feature> GetItinary(string origin, string destination)
        {
            try
            {
                Position originPos = getPositionForAddress(origin).GetAwaiter().GetResult();
                Trace.WriteLine("Origin: " + originPos.latitude + " " + originPos.longitude);
                Position destinationPos = getPositionForAddress(destination).GetAwaiter().GetResult();
                Trace.WriteLine("Destination: " + destinationPos.latitude + " " + destinationPos.longitude);
                JCDStation originStation = getNearestStation(originPos, true).GetAwaiter().GetResult();
                JCDStation destinationStation = getNearestStation(destinationPos, false).GetAwaiter().GetResult();


                var itineraries =
                    getOptimalItinary(originPos, destinationPos, originStation.position, destinationStation.position)
                        .GetAwaiter().GetResult();
        
                return itineraries;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in GetItinary: {ex.Message}");
                Trace.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
        public Boolean SendItineraryToQueue(string origin, string destination)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var itineraries = GetItinary(origin, destination);
                ActiveMQProducer producer = new ActiveMQProducer();

                foreach (var itinerary in itineraries)
                {
                    var jsonToSend = JsonSerializer.Serialize(itinerary);
                    Trace.WriteLine("Sending Itinerary: " + jsonToSend);
                    producer.SendMessage(jsonToSend);
                }
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;

                Console.WriteLine($"Time to send itinerary to queue: {elapsedTime.TotalMilliseconds} milliseconds");


                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in SendItineraryToQueue: {ex.Message}");
                return false;
            }
        }
        private async Task<Feature> getItineraryFor(Position start, Position end, string travelMode)
        {
            try
            {

                var client = new HttpClient();
                var startCoords = $"{start.longitude.ToString(CultureInfo.InvariantCulture)},{start.latitude.ToString(CultureInfo.InvariantCulture)}";
                var endCoords = $"{end.longitude.ToString(CultureInfo.InvariantCulture)},{end.latitude.ToString(CultureInfo.InvariantCulture)}";
                var requestUrl = $"https://api.openrouteservice.org/v2/directions/{travelMode}?api_key={OpenRouteServiceApiKey}&start={startCoords}&end={endCoords}";
                Trace.WriteLine("URL" + requestUrl);Trace.WriteLine("URL"+requestUrl);
                var response = await client.GetStringAsync(requestUrl);
                try
                {
                    var routeResponse = JsonSerializer.Deserialize<OpenRouteServiceResponse>(response);
                    if (routeResponse?.features == null || routeResponse.features.Count == 0)
                    {
                        throw new Exception("Route not found");
                    }
                    Trace.WriteLine("Position: " + startCoords + " " + endCoords);
                    Trace.WriteLine("Distance: "+routeResponse.features[0].properties.summary.distance);

                    return routeResponse.features[0];
                }
                catch (JsonException ex)
                {
                    Trace.WriteLine("Erreur de désérialisation JSON: " + ex.Message);
                    Trace.WriteLine("Détails: " + ex.InnerException?.Message);
                    throw;
                }

                
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in getItineraryFor: {ex.Message}");
                Trace.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
            

            
           
        }

        private async Task<List<Feature>> getOptimalItinary(Position originPos, Position destinationPos, Position station1, Position station2)
        {
            // Calcul des itinéraires
            Feature footItinerary = await getItineraryFor(originPos, destinationPos, MODE_WALKING);
            Feature originToStation1 = await getItineraryFor(originPos, station1, MODE_WALKING);
            Feature station1ToStation2 = await getItineraryFor(station1, station2, MODE_BIKING);
            Feature station2ToDestination = await getItineraryFor(station2, destinationPos, MODE_WALKING);

            // Calcul des durées
            double footDuration = footItinerary.properties.summary.duration;
            double bikeDuration = originToStation1.properties.summary.duration + station1ToStation2.properties.summary.duration + station2ToDestination.properties.summary.duration;

            // Comparaison des durées et construction de l'itinéraire optimal
            if (footDuration <= bikeDuration)
            {
                return new List<Feature> { footItinerary };
            }
            else
            {
                return new List<Feature> { originToStation1, station1ToStation2, station2ToDestination };
            }
        }
        

        private async Task<JCDStation> getNearestStation(Position position, bool isInOriginMode)
        {
            var binding = new WSHttpBinding();
            var endpoint = new EndpointAddress("http://localhost:8090/Proxy/");

            var serviceClient = new ServiceClient(binding,endpoint);
            try
            {
                JCDStation nearestStation = await serviceClient.GetNearestStationAsync(position, isInOriginMode);

                // Use the nearestStation object as needed
                return nearestStation;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error: {ex.Message}");
                throw ex;
            }
        }


        static async Task<Position> getPositionForAddress(string address)
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=jsonv2&q={Uri.EscapeDataString(address)}";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyBikeProject/1.0");

                var response = await httpClient.GetStringAsync(url);
                try
                {
                    var items = JsonSerializer.Deserialize<List<ApiResponseItem>>(response);
                    if (items != null && items.Count > 0)
                    {
                        if (Double.TryParse(items[0].lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude) &&
                    Double.TryParse(items[0].lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
                        {
                            return new Position
                            {
                                latitude = latitude,
                                longitude = longitude
                            };
                        }
                    }
                    return new Position();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error in getPositionForAddress: {ex.Message}");
                    Trace.WriteLine($"StackTrace: {ex.StackTrace}");
                    throw;
                }
            }
        }
        static async Task<string> JCDecauxAPICall(string url, string query)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url + "?" + query);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}

