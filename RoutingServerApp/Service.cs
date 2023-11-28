using System.Text.Json;
using System.Globalization;
using RoutingServer;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Device.Location;
using RoutingServerApp.OpenRouteClasses;

namespace RoutingServer
{   public class Service : IService
    {
        const string OpenRouteServiceApiKey = "5b3ce3597851110001cf6248fad8725cf09348fa913d6e0ded7dfaf5";
        const string MODE_WALKING = "foot-walking";
        const string MODE_BIKING = "foot-walking";
        public List<Route> GetItinary(string origin, string destination)
        {
            try
            {
                Position originPos = getPositionForAddress(origin).Result;
            Position destinationPos = getPositionForAddress(destination).Result;
            /*JCDStation originStation = getNearestStation(originPos, true);
            JCDStation destinationStation = getNearestStation(destinationPos,false);
            var itineraries = new List<Route>();
            if (isStationsUseful(originPos, destinationPos, originStation.position, destinationStation.position))
            {
                try
                {
                    itineraries.Add(getItineraryFor(originPos, originStation.position, MODE_WALKING).Result);
                    itineraries.Add(getItineraryFor(originStation.position,destinationStation.position, MODE_BIKING).Result);
                    itineraries.Add(getItineraryFor(destinationStation.position, destinationPos, MODE_WALKING).Result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
            }*/
            var itineraries = new List<Route>();
            
                itineraries.Add(getItineraryFor(originPos, destinationPos, MODE_WALKING).Result);
                return itineraries;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetItinary: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task<Route> getItineraryFor(Position start, Position end, string travelMode)
        {
            try
            {

                var client = new HttpClient();
                var requestUrl = $"https://api.openrouteservice.org/v2/directions/{travelMode}/geojson?api_key={OpenRouteServiceApiKey}&start={start.longitude},{start.latitude}&end={end.longitude},{end.latitude}";
                Console.WriteLine("URL"+requestUrl);
                var response = await client.GetStringAsync(requestUrl);
                var routeResponse = JsonSerializer.Deserialize<OpenRouteServiceResponse>(response);
                if (routeResponse?.Routes == null || routeResponse.Routes.Count == 0)
                {
                    throw new Exception("Route not found");
                }

                return routeResponse.Routes[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in getItineraryFor: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
            

            
           
        }

        private bool isStationsUseful(Position originPos, Position destinationPos, Position station1, Position station2)
        {
            GeoCoordinate origin = new GeoCoordinate(originPos.latitude, originPos.latitude);
            GeoCoordinate destination = new GeoCoordinate(destinationPos.latitude, destinationPos.latitude);
            GeoCoordinate station1Pos = new GeoCoordinate(station1.latitude, station1.longitude);
            GeoCoordinate station2Pos = new GeoCoordinate(station2.latitude, station2.longitude);
            double footDistance = origin.GetDistanceTo(destination);
            double bikeDistance = origin.GetDistanceTo(station1Pos) + station1Pos.GetDistanceTo(station2Pos) + station2Pos.GetDistanceTo(destination);
            if (footDistance <= bikeDistance)
            {
                return false;
            }
            return true;
        }

        private JCDStation getNearestStation(Position originPos, bool isInOriginMode)
        {
            throw new NotImplementedException();
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
                    Console.WriteLine($"Error in getPositionForAddress: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
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

