using System.Text.Json;
using System.Globalization;
using RoutingServer;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Device.Location;

namespace RoutingServer
{   public class Service : IService
    {
        public string GetItinary(string origin, string destination)
        {
            JCDContract nearestContract;
            Position originPos = getPositionForAddress(origin).Result;
            Position destinationPos = getPositionForAddress(destination).Result;
            /*nearestContract = getNearestContract(originPos, null);
            JCDStation originStation = getNearestStation(nearestContract, originPos, true);
            JCDStation destinationStation = getNearestStation(nearestContract, destinationPos,false);
            if (isStationsUseful(originPos, destinationPos, originStation.position, destinationStation.position))
            {

            }*/
            return "OriginPos:" + originPos.latitude + " " + originPos.longitude;
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

        private JCDStation getNearestStation(JCDContract nearestContract, Position originPos, bool isInOriginMode)
        {
            throw new NotImplementedException();
        }

        private JCDContract getNearestContract(Position pos, JCDContract previousContract)
        {
            return null;
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return new Position();


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

