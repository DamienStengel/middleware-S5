using RoutingServerApp.OpenRouteClasses;
using System.Collections.Generic;
using System.ServiceModel;

namespace RoutingServer
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        List<Feature> GetItinary(string origin, string destination);
    }
}
