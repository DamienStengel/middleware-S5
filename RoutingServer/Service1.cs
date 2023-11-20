using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoutingServer
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" à la fois dans le code et le fichier de configuration.
    public class Service1 : IService1
    {
        public string GetItinary(string origin, string destination)
        {
            Console.WriteLine("Starting getItinary");
            Position originPos = getPositionForAddress(origin).Result;
            Position destinationPos = getPositionForAddress(destination).Result;
            return "Working";
        }
        static async Task<Position> getPositionForAddress(string address)
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=jsonv2&q={Uri.EscapeDataString(address)}";
    Console.WriteLine("Calling API");

    using (var httpClient = new HttpClient())
    {
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyBikeProject/1.0");

        try
        {
            var response = await httpClient.GetStringAsync(url);
            Console.WriteLine(response);
            
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request failed: {e.Message}");

        }
                return new Position();
            }
        }
    }
}
