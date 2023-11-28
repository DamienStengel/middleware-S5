using RoutingServerApp.OpenRouteClasses;
using System.Collections.Generic;
using System.ServiceModel;

namespace RoutingServer
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        List<Route> GetItinary(string origin, string destination);
    }
}
